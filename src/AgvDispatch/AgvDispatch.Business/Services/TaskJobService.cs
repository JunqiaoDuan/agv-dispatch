using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Business.Messages;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Business.Specifications.TaskJobs;
using AgvDispatch.Shared.Constants;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Messages;
using AgvDispatch.Shared.Repository;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Business.Services;

/// <summary>
/// 任务管理服务实现
/// </summary>
public class TaskJobService : ITaskJobService
{
    private readonly IRepository<TaskJob> _taskRepository;
    private readonly IRepository<Agv> _agvRepository;
    private readonly IAgvRecommendationService _recommendationService;
    private readonly IMqttBrokerService _mqttBrokerService;
    private readonly ILogger<TaskJobService> _logger;

    public TaskJobService(
        IRepository<TaskJob> taskRepository,
        IRepository<Agv> agvRepository,
        IAgvRecommendationService recommendationService,
        IMqttBrokerService mqttBrokerService,
        ILogger<TaskJobService> logger)
    {
        _taskRepository = taskRepository;
        _agvRepository = agvRepository;
        _recommendationService = recommendationService;
        _mqttBrokerService = mqttBrokerService;
        _logger = logger;
    }

    #region AGV推荐/待处理

    public async Task<List<AgvRecommendation>> GetAgvRecommendationsAsync(TaskJobType taskType)
    {
        // 仅支持上料推荐
        if (taskType != TaskJobType.CallForLoading)
        {
            _logger.LogWarning("[TaskJobService] 仅支持上料推荐: Type={Type}", taskType);
            throw new InvalidOperationException($"仅支持上料推荐，当前任务类型: {taskType}");
        }

        // 调用上料推荐服务
        var recommendations = await _recommendationService.GetLoadingRecommendationsAsync(
            minBattery: TaskConstants.MinBatteryForRecommendation,
            topCount: TaskConstants.RecommendationTopCount);

        return recommendations;
    }

    public async Task<List<AgvPendingItem>> GetPendingUnloadingAgvsAsync()
    {
        var items = await _recommendationService.GetPendingUnloadingAgvsAsync();
        _logger.LogInformation("[TaskJobService] 获取等待下料小车列表成功: 总数量={Total}, 可用数量={Available}",
            items.Count, items.Count(x => x.IsAvailable));
        return items;
    }

    public async Task<List<AgvPendingItem>> GetPendingReturnAgvsAsync()
    {
        var items = await _recommendationService.GetPendingReturnAgvsAsync();
        _logger.LogInformation("[TaskJobService] 获取等待返回小车列表成功: 总数量={Total}, 可用数量={Available}",
            items.Count, items.Count(x => x.IsAvailable));
        return items;
    }

    public async Task<List<AgvPendingItem>> GetChargeableAgvsAsync()
    {
        var items = await _recommendationService.GetChargeableAgvsAsync();
        _logger.LogInformation("[TaskJobService] 获取可充电小车列表成功: 总数量={Total}, 可用数量={Available}",
            items.Count, items.Count(x => x.IsAvailable));
        return items;
    }

    #endregion

    #region 任务CRUD

    public async Task<TaskJob> CreateTaskWithAgvAsync(
        TaskJobType taskType,
        string targetStationCode,
        Guid selectedAgvId,
        Guid? userId)
    {
        // 1. 查询小车
        var agvSpec = new AgvByIdSpec(selectedAgvId);
        var agv = await _agvRepository.FirstOrDefaultAsync(agvSpec);
        if (agv == null)
        {
            _logger.LogWarning("[TaskJobService] 小车不存在: AgvId={AgvId}", selectedAgvId);
            throw new InvalidOperationException($"小车不存在: {selectedAgvId}");
        }

        // 2. 验证小车状态
        if (agv.AgvStatus != AgvStatus.Idle)
        {
            var statusMessage = agv.AgvStatus switch
            {
                AgvStatus.Running => "小车正在执行任务",
                AgvStatus.Charging => "小车正在充电",
                AgvStatus.Error => "小车故障",
                AgvStatus.Offline => "小车离线",
                _ => "小车状态不可用"
            };
            _logger.LogWarning("[TaskJobService] 小车状态不可用: AgvId={AgvId}, Status={Status}",
                selectedAgvId, agv.AgvStatus);
            throw new InvalidOperationException($"无法分配任务: {statusMessage}");
        }

        // 3. 验证小车是否在站点上
        if (!agv.CurrentStationId.HasValue)
        {
            _logger.LogWarning("[TaskJobService] 小车未在任何站点: AgvId={AgvId}", selectedAgvId);
            throw new InvalidOperationException("无法分配任务: 小车未在任何站点");
        }

        // 4. 创建任务(状态=Assigned)
        var task = new TaskJob
        {
            Id = Guid.NewGuid(),
            TaskType = taskType,
            TaskStatus = TaskJobStatus.Assigned,
            Priority = 30, // 默认优先级
            StartStationCode = agv.CurrentStationId?.ToString() ?? "", // 起点为小车当前位置
            EndStationCode = targetStationCode,
            Description = $"{taskType.ToDisplayText()} - {targetStationCode}",
            AssignedAgvId = selectedAgvId,
            AssignedBy = userId,
            AssignedAt = DateTimeOffset.UtcNow
        };
        task.OnCreate(userId);
        await _taskRepository.AddAsync(task);

        // 5. 更新小车状态为Running,记录当前任务
        agv.AgvStatus = AgvStatus.Running;
        agv.CurrentTaskId = task.Id;
        agv.OnUpdate(userId);
        await _agvRepository.UpdateAsync(agv);

        // 6. 立即发送MQTT消息
        var message = new TaskAssignMessage
        {
            TaskType = task.TaskType,
            Priority = task.Priority,
            Timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            StartStationCode = task.StartStationCode,
            EndStationCode = task.EndStationCode,
            Description = task.Description
        };
        await _mqttBrokerService.PublishTaskAssignAsync(agv.AgvCode, message);

        _logger.LogInformation("[TaskJobService] 任务已创建并分配: TaskId={Id}, AgvCode={AgvCode}, Type={Type}",
            task.Id, agv.AgvCode, taskType);

        return task;
    }

    public async Task<TaskJob?> GetTaskByIdAsync(Guid id)
    {
        var spec = new TaskByIdSpec(id);
        return await _taskRepository.FirstOrDefaultAsync(spec);
    }

    public async Task<bool> CancelTaskAsync(Guid taskId, string? reason, Guid? userId)
    {
        throw new NotImplementedException();
    }

    #endregion

}
