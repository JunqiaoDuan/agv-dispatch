using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Business.Messages;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Business.Specifications.TaskJobs;
using AgvDispatch.Shared.Constants;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Messages;
using AgvDispatch.Shared.Repository;
using MassTransit;
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
    private readonly IPathLockService _pathLockService;
    private readonly ILogger<TaskJobService> _logger;

    public TaskJobService(
        IRepository<TaskJob> taskRepository,
        IRepository<Agv> agvRepository,
        IAgvRecommendationService recommendationService,
        IMqttBrokerService mqttBrokerService,
        IPathLockService pathLockService,
        ILogger<TaskJobService> logger)
    {
        _taskRepository = taskRepository;
        _agvRepository = agvRepository;
        _recommendationService = recommendationService;
        _mqttBrokerService = mqttBrokerService;
        _pathLockService = pathLockService;
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
        string selectedAgvCode,
        Guid? userId,
        bool? hasCargo = null)
    {
        // 1. 查询小车
        var agvSpec = new AgvByAgvCodeSpec(selectedAgvCode);
        var agv = await _agvRepository.FirstOrDefaultAsync(agvSpec);
        if (agv == null)
        {
            _logger.LogWarning("[TaskJobService] 小车不存在: AgvCode={AgvCode}", selectedAgvCode);
            throw new InvalidOperationException($"小车不存在: {selectedAgvCode}");
        }

        // 2. 验证小车状态
        if (agv.AgvStatus != AgvStatus.Online)
        {
            _logger.LogWarning("[TaskJobService] 小车离线: AgvCode={AgvCode}", selectedAgvCode);
            throw new InvalidOperationException("无法分配任务: 小车离线");
        }

        // 检查是否有运行中任务
        var runningTaskSpec = new TaskRunningByAgvCodeSpec(selectedAgvCode);
        var hasRunningTask = await _taskRepository.AnyAsync(runningTaskSpec);
        if (hasRunningTask)
        {
            _logger.LogWarning("[TaskJobService] 小车正在执行任务: AgvCode={AgvCode}", selectedAgvCode);
            throw new InvalidOperationException("无法分配任务: 小车正在执行其他任务");
        }

        // 3. 验证小车是否在站点上
        if (string.IsNullOrEmpty(agv.CurrentStationCode))
        {
            _logger.LogWarning("[TaskJobService] 小车未在任何站点: AgvCode={AgvCode}", selectedAgvCode);
            throw new InvalidOperationException("无法分配任务: 小车未在任何站点");
        }

        // 4. 创建任务(状态=Assigned)
        var task = new TaskJob
        {
            Id = NewId.NextSequentialGuid(),
            TaskType = taskType,
            TaskStatus = TaskJobStatus.Assigned,
            Priority = 30, // 默认优先级
            StartStationCode = agv.CurrentStationCode, // 起点为小车当前位置
            EndStationCode = targetStationCode,
            Description = $"{taskType.ToDisplayText()} - {targetStationCode}",
            AssignedAgvCode = selectedAgvCode,
            AssignedBy = userId,
            AssignedAt = DateTimeOffset.UtcNow
        };
        task.OnCreate(userId);
        await _taskRepository.AddAsync(task);

        // 5. 如果传入了HasCargo参数，更新小车货物状态
        if (hasCargo.HasValue && agv.HasCargo != hasCargo.Value)
        {
            agv.HasCargo = hasCargo.Value;
            _logger.LogInformation("[TaskJobService] 更新小车货物状态: AgvCode={AgvCode}, HasCargo={HasCargo}",
                agv.AgvCode, hasCargo.Value);
        }

        // 6. 更新小车修改时间
        agv.OnUpdate(userId);
        await _agvRepository.UpdateAsync(agv);

        // 7. 立即发送MQTT消息
        var message = new TaskAssignMessage
        {
            TaskId = task.Id.ToString(),
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

    public async Task<(bool Success, string? Message)> CancelTaskAsync(Guid taskId, string? reason, Guid? userId)
    {
        // 1. 查询任务
        var spec = new TaskByIdSpec(taskId);
        var task = await _taskRepository.FirstOrDefaultAsync(spec);
        if (task == null)
        {
            _logger.LogWarning("[TaskJobService] 任务不存在: TaskId={TaskId}", taskId);
            return (false, "任务不存在");
        }

        // 2. 验证任务状态：只允许取消未完成的任务（Pending, Assigned, Executing）
        // 已完成/已取消/失败的任务不能再次取消
        if (task.TaskStatus == TaskJobStatus.Completed)
        {
            _logger.LogWarning("[TaskJobService] 任务已完成，不能取消: TaskId={TaskId}", taskId);
            return (false, "任务已完成，无法取消");
        }

        if (task.TaskStatus == TaskJobStatus.Cancelled)
        {
            _logger.LogWarning("[TaskJobService] 任务已取消: TaskId={TaskId}", taskId);
            return (false, "任务已取消");
        }

        if (task.TaskStatus == TaskJobStatus.Failed)
        {
            _logger.LogWarning("[TaskJobService] 任务已失败: TaskId={TaskId}", taskId);
            return (false, "任务已失败");
        }

        // 3. 更新任务状态为 Cancelled
        task.TaskStatus = TaskJobStatus.Cancelled;
        task.CompletedAt = DateTimeOffset.UtcNow;
        task.OnUpdate(userId);
        await _taskRepository.UpdateAsync(task);

        // 4. 如果任务已分配给 AGV，需要释放 AGV 并发送取消消息
        if (!string.IsNullOrEmpty(task.AssignedAgvCode))
        {
            var agvSpec = new AgvByAgvCodeSpec(task.AssignedAgvCode);
            var agv = await _agvRepository.FirstOrDefaultAsync(agvSpec);
            if (agv != null)
            {
                // 无需修改 AGV 状态（任务状态已足够）
                agv.OnUpdate(userId);
                await _agvRepository.UpdateAsync(agv);

                // 清理路径锁定
                await _pathLockService.ClearAgvLocksAsync(task.AssignedAgvCode);

                // 发送 MQTT 取消消息
                var cancelMessage = new TaskCancelMessage
                {
                    TaskId = task.Id.ToString(),
                    Timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Reason = reason
                };
                await _mqttBrokerService.PublishTaskCancelAsync(agv.AgvCode, cancelMessage);

                _logger.LogInformation("[TaskJobService] 任务已取消并释放AGV: TaskId={TaskId}, AgvCode={AgvCode}, Reason={Reason}",
                    taskId, agv.AgvCode, reason);
            }
        }
        else
        {
            _logger.LogInformation("[TaskJobService] 任务已取消（未分配AGV）: TaskId={TaskId}", taskId);
        }

        return (true, null);
    }

    #endregion

}
