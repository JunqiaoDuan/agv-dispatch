using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.StationAggregate;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Business.Messages;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Business.Specifications.Stations;
using AgvDispatch.Business.Specifications.TaskJobs;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Repository;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Business.Services;

/// <summary>
/// 小车推荐服务实现
/// </summary>
public class AgvRecommendationService : IAgvRecommendationService
{
    private readonly IRepository<Agv> _agvRepository;
    private readonly IRepository<Station> _stationRepository;
    private readonly IRepository<TaskJob> _taskRepository;
    private readonly ILogger<AgvRecommendationService> _logger;

    // 评分权重
    private const double StatusWeight = 0.50;
    private const double StationPriorityWeight = 0.30;
    private const double BatteryWeight = 0.20;

    public AgvRecommendationService(
        IRepository<Agv> agvRepository,
        IRepository<Station> stationRepository,
        IRepository<TaskJob> taskRepository,
        ILogger<AgvRecommendationService> logger)
    {
        _agvRepository = agvRepository;
        _stationRepository = stationRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    #region 接口实现

    /// <summary>
    /// 获取上料推荐小车列表
    /// </summary>
    public async Task<List<AgvRecommendation>> GetLoadingRecommendationsAsync(
        int minBattery,
        int topCount)
    {
        // 1. 获取所有小车
        var agvSpec = new AgvListSpec();
        var allAgvs = await _agvRepository.ListAsync(agvSpec);

        // 2. 批量查询所有运行中的任务（性能优化）
        var runningTasksSpec = new TaskRunningSpec();
        var runningTasks = await _taskRepository.ListAsync(runningTasksSpec);
        var busyAgvCodes = new HashSet<string>(
            runningTasks
                .Where(t => !string.IsNullOrEmpty(t.AssignedAgvCode))
                .Select(t => t.AssignedAgvCode!),
            StringComparer.OrdinalIgnoreCase
        );

        // 3. 为所有小车生成推荐结果
        var recommendations = new List<AgvRecommendation>();
        foreach (var agv in allAgvs)
        {
            // 获取当前站点信息
            Station? currentStation = null;
            if (!string.IsNullOrEmpty(agv.CurrentStationCode))
            {
                var stationSpec = new StationByStationCodeSpec(agv.CurrentStationCode);
                currentStation = await _stationRepository.FirstOrDefaultAsync(stationSpec);
            }

            // 判断是否可用及原因
            var (isAvailable, reasons) = EvaluateLoadingAvailability(agv, currentStation, minBattery, busyAgvCodes);

            // 计算评分（只对可用的小车计算评分，不可用的评分为0）
            var recommendation = isAvailable
                ? await CalculateScoreAsync(agv, minBattery, busyAgvCodes)
                : CreateUnavailableRecommendation(agv, currentStation);

            recommendation.IsAvailable = isAvailable;
            recommendation.RecommendReason = string.Join(", ", reasons);

            recommendations.Add(recommendation);
        }

        // 4. 按可用性和总分排序：可用的在前，不可用的在后；可用的按评分降序排列
        var sortedRecommendations = recommendations
            .OrderByDescending(x => x.IsAvailable)
            .ThenByDescending(x => x.TotalScore)
            .ToList();

        _logger.LogInformation("[Loading] 推荐完成: 总数量={Total}, 可用数量={Available}",
            sortedRecommendations.Count, sortedRecommendations.Count(x => x.IsAvailable));

        return sortedRecommendations;
    }

    /// <summary>
    /// 获取等待下料的小车列表
    /// </summary>
    public async Task<List<AgvPendingItem>> GetPendingUnloadingAgvsAsync()
    {
        // 获取所有小车
        var agvSpec = new AgvListSpec();
        var allAgvs = await _agvRepository.ListAsync(agvSpec);

        var pendingItems = new List<AgvPendingItem>();
        foreach (var agv in allAgvs)
        {
            // 获取当前站点信息
            Station? currentStation = null;
            if (!string.IsNullOrEmpty(agv.CurrentStationCode))
            {
                var stationSpec = new StationByStationCodeSpec(agv.CurrentStationCode);
                currentStation = await _stationRepository.FirstOrDefaultAsync(stationSpec);
            }

            // 判断是否可用及原因
            var (isAvailable, reason) = EvaluateUnloadingAvailability(agv, currentStation);

            pendingItems.Add(new AgvPendingItem
            {
                AgvId = agv.Id,
                AgvCode = agv.AgvCode,
                IsAvailable = isAvailable,
                Reason = reason,
                Battery = agv.Battery,
                HasCargo = agv.HasCargo,
                AgvStatus = agv.AgvStatus,
                CurrentStationCode = currentStation?.StationCode,
                CurrentStationName = currentStation?.DisplayName,
                CurrentStationType = currentStation?.StationType
            });
        }

        // 按可用性排序：可用的在前
        var sortedItems = pendingItems
            .OrderByDescending(x => x.IsAvailable)
            .ToList();

        _logger.LogInformation("[Unloading] 总数量: {Total}, 可用数量: {Available}",
            sortedItems.Count, sortedItems.Count(x => x.IsAvailable));

        return sortedItems;
    }

    /// <summary>
    /// 获取等待返回的小车列表
    /// </summary>
    public async Task<List<AgvPendingItem>> GetPendingReturnAgvsAsync()
    {
        // 获取所有小车
        var agvSpec = new AgvListSpec();
        var allAgvs = await _agvRepository.ListAsync(agvSpec);

        var pendingItems = new List<AgvPendingItem>();
        foreach (var agv in allAgvs)
        {
            // 获取当前站点信息
            Station? currentStation = null;
            if (!string.IsNullOrEmpty(agv.CurrentStationCode))
            {
                var stationSpec = new StationByStationCodeSpec(agv.CurrentStationCode);
                currentStation = await _stationRepository.FirstOrDefaultAsync(stationSpec);
            }

            // 判断是否可用及原因
            var (isAvailable, reason) = EvaluateReturnAvailability(agv);

            pendingItems.Add(new AgvPendingItem
            {
                AgvId = agv.Id,
                AgvCode = agv.AgvCode,
                IsAvailable = isAvailable,
                Reason = reason,
                Battery = agv.Battery,
                HasCargo = agv.HasCargo,
                AgvStatus = agv.AgvStatus,
                CurrentStationCode = currentStation?.StationCode,
                CurrentStationName = currentStation?.DisplayName,
                CurrentStationType = currentStation?.StationType
            });
        }

        // 按可用性排序：可用的在前
        var sortedItems = pendingItems
            .OrderByDescending(x => x.IsAvailable)
            .ToList();

        _logger.LogInformation("[Return] 总数量: {Total}, 可用数量: {Available}",
            sortedItems.Count, sortedItems.Count(x => x.IsAvailable));

        return sortedItems;
    }

    /// <summary>
    /// 获取可充电的小车列表
    /// </summary>
    public async Task<List<AgvPendingItem>> GetChargeableAgvsAsync()
    {
        // 获取所有小车
        var agvSpec = new AgvListSpec();
        var allAgvs = await _agvRepository.ListAsync(agvSpec);

        var pendingItems = new List<AgvPendingItem>();
        foreach (var agv in allAgvs)
        {
            // 获取当前站点信息
            Station? currentStation = null;
            if (!string.IsNullOrEmpty(agv.CurrentStationCode))
            {
                var stationSpec = new StationByStationCodeSpec(agv.CurrentStationCode);
                currentStation = await _stationRepository.FirstOrDefaultAsync(stationSpec);
            }

            // 判断是否可用及原因
            var (isAvailable, reason) = EvaluateChargeableAvailability(agv);

            pendingItems.Add(new AgvPendingItem
            {
                AgvId = agv.Id,
                AgvCode = agv.AgvCode,
                IsAvailable = isAvailable,
                Reason = reason,
                Battery = agv.Battery,
                HasCargo = agv.HasCargo,
                AgvStatus = agv.AgvStatus,
                CurrentStationCode = currentStation?.StationCode,
                CurrentStationName = currentStation?.DisplayName,
                CurrentStationType = currentStation?.StationType
            });
        }

        // 按可用性和电量排序：可用的在前，可用的按电量升序（电量低的优先充电）
        var sortedItems = pendingItems
            .OrderByDescending(x => x.IsAvailable)
            .ThenBy(x => x.Battery)
            .ToList();

        _logger.LogInformation("[Charge] 总数量: {Total}, 可用数量: {Available}",
            sortedItems.Count, sortedItems.Count(x => x.IsAvailable));

        return sortedItems;
    }

    #endregion

    #region 计算评分

    private async Task<AgvRecommendation> CalculateScoreAsync(Agv agv, int minBattery, HashSet<string> busyAgvCodes)
    {

        // 计算各项评分 (每项按100分计算,然后乘以权重)
        var batteryBaseScore = CalculateBatteryBaseScore(agv.Battery, minBattery);
        var statusBaseScore = CalculateStatusBaseScore(agv, busyAgvCodes);

        // 站点优先级评分（仅针对空闲小车）
        var stationPriorityBaseScore = 0.0;
        string? currentStationCode = null;
        int? currentStationPriority = null;
        Station? currentStation = null;

        // 只有在线且无运行任务的小车才计算站点优先级
        bool isIdle = agv.AgvStatus == AgvStatus.Online
                   && !busyAgvCodes.Contains(agv.AgvCode);

        if (isIdle && !string.IsNullOrEmpty(agv.CurrentStationCode))
        {
            var stationSpec = new StationByStationCodeSpec(agv.CurrentStationCode);
            currentStation = await _stationRepository.FirstOrDefaultAsync(stationSpec);
            if (currentStation != null)
            {
                currentStationCode = currentStation.StationCode;
                currentStationPriority = currentStation.Priority;
                stationPriorityBaseScore = CalculateStationPriorityBaseScore(currentStation.Priority);
            }
        }

        // 应用权重计算最终评分
        var batteryScore = batteryBaseScore * BatteryWeight;
        var statusScore = statusBaseScore * StatusWeight;
        var stationPriorityScore = stationPriorityBaseScore * StationPriorityWeight;

        // 总分
        var totalScore = batteryScore + statusScore + stationPriorityScore;

        // 生成简化的推荐理由
        var reason = GenerateRecommendReason(agv, currentStation, minBattery, busyAgvCodes);

        return new AgvRecommendation
        {
            AgvId = agv.Id,
            AgvCode = agv.AgvCode,
            IsAvailable = true,
            TotalScore = Math.Round(totalScore, 2),
            BatteryScore = Math.Round(batteryScore, 2),
            StatusScore = Math.Round(statusScore, 2),
            StationPriorityScore = Math.Round(stationPriorityScore, 2),
            Battery = agv.Battery,
            Status = agv.AgvStatus,
            HasCargo = agv.HasCargo,
            CurrentStationCode = currentStationCode,
            CurrentStationPriority = currentStationPriority,
            RecommendReason = reason
        };
    }

    /// <summary>
    /// 计算电量基础评分(0-100分)
    /// 电量越高分数越高
    /// </summary>
    private double CalculateBatteryBaseScore(int battery, int minBattery)
    {
        if (battery >= 80)
            return 100.0; // 电量充足

        if (battery >= 60)
            return 80.0; // 电量良好

        if (battery >= minBattery)
            return 50.0; // 电量可用

        // 低于最低要求严重扣分
        return 20.0;
    }

    /// <summary>
    /// 计算状态基础评分(0-100分)
    /// </summary>
    private double CalculateStatusBaseScore(Agv agv, HashSet<string> busyAgvCodes)
    {
        // 离线 -> 0分
        if (agv.AgvStatus == AgvStatus.Offline)
            return 0.0;

        // 执行任务中 -> 15分
        if (busyAgvCodes.Contains(agv.AgvCode))
            return 15.0;

        // 空闲 -> 100分
        return 100.0;
    }

    /// <summary>
    /// 计算站点优先级基础评分(0-100分)
    /// 仅针对空闲状态的小车计算
    /// Priority 值越大优先级越高，分数越高
    /// </summary>
    private double CalculateStationPriorityBaseScore(int priority)
    {
        // Priority 范围为 0-100，直接作为基础评分
        return Math.Clamp(priority, 0, 100);
    }

    /// <summary>
    /// 生成推荐理由
    /// 直接根据原始数据（状态、电量、站点优先级）生成理由，与评分算法解耦
    /// </summary>
    private string GenerateRecommendReason(Agv agv, Station? currentStation, int minBattery, HashSet<string> busyAgvCodes)
    {
        var reasons = new List<string>();

        // 状态因素
        if (agv.AgvStatus == AgvStatus.Offline)
        {
            reasons.Add("离线");
        }
        else if (busyAgvCodes.Contains(agv.AgvCode))
        {
            reasons.Add("正在执行任务");
        }
        else
        {
            reasons.Add("空闲可用");
        }

        // 站点优先级因素 - 直接根据 Priority 值（仅对空闲小车）
        bool isIdle = agv.AgvStatus == AgvStatus.Online && !busyAgvCodes.Contains(agv.AgvCode);
        if (isIdle && currentStation != null)
        {
            if (currentStation.Priority >= 80)
                reasons.Add("停靠站点优先级很高");
            else if (currentStation.Priority >= 60)
                reasons.Add("停靠站点优先级较高");
            else if (currentStation.Priority >= 40)
                reasons.Add("停靠站点优先级中等");
            else if (currentStation.Priority >= 20)
                reasons.Add("停靠站点优先级较低");
            else if (currentStation.Priority > 0)
                reasons.Add("停靠站点优先级低");
        }

        // 电量因素 - 直接根据电量百分比
        if (agv.Battery >= 80)
            reasons.Add("电量充足");
        else if (agv.Battery >= 60)
            reasons.Add("电量良好");
        else if (agv.Battery >= minBattery)
            reasons.Add("电量可用");
        else
            reasons.Add("电量偏低");

        return string.Join(", ", reasons);
    }

    #endregion

    #region 可用性评估

    /// <summary>
    /// 评估上料可用性
    /// 筛选条件：Online + 无运行任务 + Standby站点 + !HasCargo + Battery≥阈值
    /// </summary>
    private (bool isAvailable, List<string> reasons) EvaluateLoadingAvailability(
        Agv agv,
        Station? currentStation,
        int minBattery,
        HashSet<string> busyAgvCodes)
    {
        var reasons = new List<string>();
        var isAvailable = true;

        // 检查在线状态
        if (agv.AgvStatus != AgvStatus.Online)
        {
            isAvailable = false;
            reasons.Add("小车离线");
        }
        else if (busyAgvCodes.Contains(agv.AgvCode))
        {
            isAvailable = false;
            reasons.Add("小车正在执行任务");
        }
        else
        {
            reasons.Add("小车空闲");
        }

        // 检查是否有货物
        if (agv.HasCargo)
        {
            isAvailable = false;
            reasons.Add("小车已载货");
        }
        else
        {
            reasons.Add("小车未载货");
        }

        // 检查电量
        if (agv.Battery < minBattery)
        {
            isAvailable = false;
            reasons.Add($"电量不足(当前{agv.Battery}%,需要≥{minBattery}%)");
        }
        else if (agv.Battery >= 80)
        {
            reasons.Add("电量充足");
        }
        else if (agv.Battery >= 60)
        {
            reasons.Add("电量良好");
        }
        else
        {
            reasons.Add("电量可用");
        }

        // 检查当前站点
        if (string.IsNullOrEmpty(agv.CurrentStationCode))
        {
            isAvailable = false;
            reasons.Add("未在任何站点");
        }
        else if (currentStation == null)
        {
            isAvailable = false;
            reasons.Add("当前站点信息不存在");
        }
        else if (currentStation.StationType != StationType.Standby)
        {
            isAvailable = false;
            reasons.Add($"请先将小车调到【待命站点】");
        }
        else
        {
            reasons.Add($"位于待命站点({currentStation.StationCode})");
            // 添加站点优先级信息
            if (currentStation.Priority >= 80)
                reasons.Add("站点优先级很高");
            else if (currentStation.Priority >= 60)
                reasons.Add("站点优先级较高");
            else if (currentStation.Priority >= 40)
                reasons.Add("站点优先级中等");
        }

        return (isAvailable, reasons);
    }

    /// <summary>
    /// 评估下料可用性
    /// 筛选条件：Idle + Pickup站点 + !HasCargo
    /// </summary>
    private (bool isAvailable, string reason) EvaluateUnloadingAvailability(
        Agv agv,
        Station? currentStation)
    {
        var reasons = new List<string>();
        var isAvailable = true;

        // 检查状态
        if (agv.AgvStatus != AgvStatus.Online)
        {
            isAvailable = false;
            reasons.Add("小车离线");
        }
        else
        {
            reasons.Add("小车在线");
        }

        // 检查是否有货物
        if (agv.HasCargo)
        {
            isAvailable = false;
            reasons.Add("小车已确认载货");
        }
        else
        {
            reasons.Add("小车未确认载货");
        }

        // 检查当前站点
        if (string.IsNullOrEmpty(agv.CurrentStationCode))
        {
            isAvailable = false;
            reasons.Add("未在任何站点");
        }
        else if (currentStation == null)
        {
            isAvailable = false;
            reasons.Add("当前站点信息不存在");
        }
        else if (currentStation.StationType != StationType.Pickup)
        {
            isAvailable = false;
            reasons.Add($"当前站点类型为{currentStation.StationType},需要在Pickup站点");
        }
        else
        {
            reasons.Add($"位于Pickup站点({currentStation.StationCode}),等待下料");
        }

        return (isAvailable, string.Join(", ", reasons));
    }

    /// <summary>
    /// 评估返回可用性
    /// 筛选条件：Idle + HasCargo
    /// </summary>
    private (bool isAvailable, string reason) EvaluateReturnAvailability(Agv agv)
    {
        var reasons = new List<string>();
        var isAvailable = true;

        // 检查状态
        if (agv.AgvStatus != AgvStatus.Online)
        {
            isAvailable = false;
            reasons.Add("小车离线");
        }
        else
        {
            reasons.Add("小车在线");
        }

        return (isAvailable, string.Join(", ", reasons));
    }

    /// <summary>
    /// 评估充电可用性
    /// 筛选条件：Idle
    /// </summary>
    private (bool isAvailable, string reason) EvaluateChargeableAvailability(Agv agv)
    {
        var reasons = new List<string>();
        var isAvailable = true;

        // 检查状态
        if (agv.AgvStatus != AgvStatus.Online)
        {
            isAvailable = false;
            reasons.Add("小车离线");
        }
        else
        {
            reasons.Add("小车在线");
        }

        // 添加电量信息
        if (agv.Battery < 20)
            reasons.Add("电量低,建议充电");
        else if (agv.Battery < 60)
            reasons.Add("电量中等");
        else if (agv.Battery < 80)
            reasons.Add("电量良好");
        else
            reasons.Add("电量充足");

        return (isAvailable, string.Join(", ", reasons));
    }

    /// <summary>
    /// 创建不可用的推荐结果
    /// </summary>
    private AgvRecommendation CreateUnavailableRecommendation(Agv agv, Station? currentStation)
    {
        return new AgvRecommendation
        {
            AgvId = agv.Id,
            AgvCode = agv.AgvCode,
            IsAvailable = false,
            TotalScore = 0,
            BatteryScore = 0,
            StatusScore = 0,
            StationPriorityScore = 0,
            Battery = agv.Battery,
            Status = agv.AgvStatus,
            HasCargo = agv.HasCargo,
            CurrentStationCode = currentStation?.StationCode,
            CurrentStationPriority = currentStation?.Priority,
            RecommendReason = string.Empty
        };
    }

    #endregion

}
