using AgvDispatch.Shared.Messages;
using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Shared.Repository;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Business.Specifications.TaskJobs;
using AgvDispatch.Business.Services;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Business.Entities.StationAggregate;
using AgvDispatch.Business.Specifications.Stations;

namespace AgvDispatch.Infrastructure.Mqtt;

/// <summary>
/// MQTT 消息处理器实现
///
/// 注意：本服务注册为 Singleton，不能直接注入 Scoped 的 DbContext/Repository，
/// 需通过 IServiceScopeFactory 在使用时创建 scope 来获取
/// </summary>
public class MqttMessageHandler : IMqttMessageHandler
{
    private readonly ILogger<MqttMessageHandler> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MqttMessageHandler(
        ILogger<MqttMessageHandler> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// 处理状态上报消息
    /// 职责：只更新小车的物理状态（电量、速度、位置等）
    /// 不更新：AgvStatus（由任务/异常驱动）、CurrentTaskId（由进度管理）、ErrorCode（由异常管理）
    /// </summary>
    public async Task HandleStatusAsync(string agvCode, StatusMessage message)
    {
        _logger.LogInformation(
            "[MqttMessageHandler] 收到小车 {AgvCode} 的状态上报: BatteryVoltage={BatteryVoltage}V, Speed={Speed}m/s, Position=({X},{Y},{Angle}°), Station={Station}",
            agvCode, message.BatteryVoltage, message.Speed, message.Position.X, message.Position.Y, message.Position.Angle, message.Position.StationCode);

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var agvRepository = scope.ServiceProvider.GetRequiredService<IRepository<Agv>>();

            // 查询小车
            var spec = new AgvByAgvCodeSpec(agvCode);
            var agv = await agvRepository.FirstOrDefaultAsync(spec);

            if (agv == null)
            {
                _logger.LogError("[MqttMessageHandler] 小车 {AgvCode} 不存在，无法更新状态", agvCode);
                return;
            }

            // ========== 只更新物理状态 ==========
            // 电量：使用电压值计算百分比
            // 根据电压值计算电量百分比
            agv.Battery = AgvConstants.CalculateBatteryPercentage((decimal)message.BatteryVoltage);
            agv.BatteryVoltage = (decimal)message.BatteryVoltage;

            _logger.LogDebug("[MqttMessageHandler] 小车 {AgvCode} 电池电压={BatteryVoltage}V, 计算得电量={Battery}%",
                agvCode, message.BatteryVoltage, agv.Battery);

            // 速度
            agv.Speed = (decimal)message.Speed;

            // 当前站点编码
            agv.CurrentStationCode = message.Position.StationCode;

            // 位置信息
            if (message.Position.X.HasValue)
                agv.PositionX = (decimal)message.Position.X.Value;

            if (message.Position.Y.HasValue)
                agv.PositionY = (decimal)message.Position.Y.Value;

            if (message.Position.Angle.HasValue)
                agv.PositionAngle = (decimal)message.Position.Angle.Value;

            // 最后在线时间
            agv.LastOnlineTime = DateTimeOffset.UtcNow;

            // 更新在线状态
            if (agv.AgvStatus == AgvStatus.Offline)
            {
                agv.AgvStatus = AgvStatus.Online;
            }

            // 保存更新
            await agvRepository.UpdateAsync(agv);

            _logger.LogDebug("[MqttMessageHandler] 小车 {AgvCode} 物理状态已更新", agvCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MqttMessageHandler] 更新小车 {AgvCode} 状态失败", agvCode);
        }
    }

    /// <summary>
    /// 处理任务进度消息
    /// 职责：更新任务状态、进度，创建进度日志，联动更新小车的 CurrentTaskId 和 AgvStatus
    /// </summary>
    public async Task HandleTaskProgressAsync(string agvCode, TaskProgressMessage message)
    {
        _logger.LogInformation("[MqttMessageHandler] 收到小车 {AgvCode} 的任务进度: TaskId={TaskId}, Status={Status}, Progress={Progress}%",
            agvCode, message.TaskId, message.Status, message.ProgressPercentage);

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<IRepository<TaskJob>>();
            var agvRepository = scope.ServiceProvider.GetRequiredService<IRepository<Agv>>();
            var progressLogRepository = scope.ServiceProvider.GetRequiredService<IRepository<TaskProgressLog>>();

            // 解析任务ID
            if (!Guid.TryParse(message.TaskId, out var taskId))
            {
                _logger.LogError("[MqttMessageHandler] 无效的任务ID: {TaskId}", message.TaskId);
                return;
            }

            // 查询任务
            var taskSpec = new TaskByIdSpec(taskId);
            var task = await taskRepository.FirstOrDefaultAsync(taskSpec);
            if (task == null)
            {
                _logger.LogError("[MqttMessageHandler] 任务 {TaskId} 不存在", message.TaskId);
                return;
            }

            // 查询小车
            var agvSpec = new AgvByAgvCodeSpec(agvCode);
            var agv = await agvRepository.FirstOrDefaultAsync(agvSpec);
            if (agv == null)
            {
                _logger.LogError("[MqttMessageHandler] 小车 {AgvCode} 不存在", agvCode);
                return;
            }

            // ========== 检测变化 ==========
            var oldStatus = task.TaskStatus;
            var oldProgress = task.ProgressPercentage;
            var statusChanged = oldStatus != message.Status;
            var newProgress = message.ProgressPercentage.HasValue ? (decimal)message.ProgressPercentage.Value : (decimal?)null;

            // 进度变化超过1%才认为有意义（避免浮点误差和频繁更新）
            var progressChanged = newProgress.HasValue
                && (!oldProgress.HasValue || Math.Abs(newProgress.Value - oldProgress.Value) >= 1);

            // 如果状态和进度都没有实质性变化，仅记录Debug日志，不更新数据库
            if (!statusChanged && !progressChanged)
            {
                _logger.LogDebug("[MqttMessageHandler] 任务 {TaskId} 状态和进度无变化，跳过更新", message.TaskId);
                return;
            }

            // ========== 更新任务状态和进度 ==========
            var taskNeedUpdate = false;

            if (statusChanged)
            {
                task.TaskStatus = message.Status;
                taskNeedUpdate = true;

                // 根据状态转换更新时间字段
                if (message.Status == TaskJobStatus.Executing && oldStatus != TaskJobStatus.Executing)
                {
                    task.StartedAt = DateTimeOffset.UtcNow;
                    _logger.LogInformation("[MqttMessageHandler] 任务 {TaskId} 开始执行", message.TaskId);
                }
                else if (message.Status == TaskJobStatus.Completed && oldStatus != TaskJobStatus.Completed)
                {
                    task.CompletedAt = DateTimeOffset.UtcNow;
                    task.ProgressPercentage = 100;
                    _logger.LogInformation("[MqttMessageHandler] 任务 {TaskId} 已完成", message.TaskId);
                }
                else if (message.Status == TaskJobStatus.Failed && oldStatus != TaskJobStatus.Failed)
                {
                    task.FailureReason = message.Message ?? "任务执行失败";
                    _logger.LogWarning("[MqttMessageHandler] 任务 {TaskId} 执行失败: {Reason}", message.TaskId, task.FailureReason);
                }
                else if (message.Status == TaskJobStatus.Cancelled && oldStatus != TaskJobStatus.Cancelled)
                {
                    task.CancelledAt = DateTimeOffset.UtcNow;
                    task.CancelReason = message.Message ?? "任务已取消";
                    _logger.LogInformation("[MqttMessageHandler] 任务 {TaskId} 已取消", message.TaskId);
                }
            }

            if (progressChanged)
            {
                task.ProgressPercentage = newProgress;
                taskNeedUpdate = true;
                _logger.LogDebug("[MqttMessageHandler] 任务 {TaskId} 进度更新: {OldProgress}% → {NewProgress}%",
                    message.TaskId, oldProgress, newProgress);
            }

            if (taskNeedUpdate)
            {
                await taskRepository.UpdateAsync(task);
            }

            // ========== 联动更新小车状态（仅在状态转换时） ==========
            // 任务完成或失败时，AGV 状态不再需要修改（通过任务状态就能判断 AGV 是否空闲）
            // 删除所有对 AGV 状态的修改

            // ========== 创建任务进度日志（只在有意义的变化时创建） ==========
            // 状态变化或进度变化时才记录
            if (statusChanged || progressChanged)
            {
                var progressLog = new TaskProgressLog
                {
                    TaskId = message.TaskId,
                    AgvCode = agvCode,
                    Status = message.Status,
                    ProgressPercentage = message.ProgressPercentage,
                    Message = message.Message,
                    ReportTime = DateTimeOffset.UtcNow
                };
                progressLog.OnCreate();
                await progressLogRepository.AddAsync(progressLog);

                _logger.LogDebug("[MqttMessageHandler] 任务进度日志已创建: Status={Status}, Progress={Progress}%",
                    message.Status.ToDisplayText(), message.ProgressPercentage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MqttMessageHandler] 处理任务进度失败: TaskId={TaskId}, AgvCode={AgvCode}", message.TaskId, agvCode);
        }
    }

    /// <summary>
    /// 处理异常上报消息
    /// 职责：创建异常日志，记录到数据库，待人工处理
    /// </summary>
    public async Task HandleExceptionAsync(string agvCode, ExceptionMessage message)
    {
        _logger.LogWarning("[MqttMessageHandler] 收到小车 {AgvCode} 的异常上报: Type={Type}, Severity={Severity}, Message={Message}",
            agvCode, message.ExceptionType.ToDisplayText(), message.Severity.ToDisplayText(), message.Message);

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var agvRepository = scope.ServiceProvider.GetRequiredService<IRepository<Agv>>();
            var exceptionLogRepository = scope.ServiceProvider.GetRequiredService<IRepository<AgvExceptionLog>>();

            // 查询小车
            var agvSpec = new AgvByAgvCodeSpec(agvCode);
            var agv = await agvRepository.FirstOrDefaultAsync(agvSpec);
            if (agv == null)
            {
                _logger.LogError("[MqttMessageHandler] 小车 {AgvCode} 不存在", agvCode);
                return;
            }

            // ========== 创建异常日志（只存MQTT消息原样） ==========
            // 根据严重级别决定是否需要人工处理：
            // - Error/Critical：需要人工处理（IsResolved = false）
            // - Info/Warning：自动记录为已完成（IsResolved = true）
            var needsManualResolution = message.Severity == AgvExceptionSeverity.Error
                                      || message.Severity == AgvExceptionSeverity.Critical;

            var exceptionLog = new AgvExceptionLog
            {
                AgvCode = agvCode,
                TaskId = message.TaskId,  // 字符串格式
                ExceptionType = message.ExceptionType,
                Severity = message.Severity,
                Message = message.Message,
                PositionX = message.Position?.X.HasValue == true ? (decimal)message.Position.X.Value : null,
                PositionY = message.Position?.Y.HasValue == true ? (decimal)message.Position.Y.Value : null,
                PositionAngle = message.Position?.Angle.HasValue == true ? (decimal)message.Position.Angle.Value : null,
                StationCode = message.Position?.StationCode,  // 站点ID（来自MQTT消息）
                ExceptionTime = DateTimeOffset.UtcNow,
                ErrorCode = $"{message.ExceptionType}_{message.Severity}_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
                IsResolved = !needsManualResolution,  // Error/Critical=false需人工处理，Info/Warning=true已自动完成
                ResolvedTime = needsManualResolution ? null : DateTimeOffset.UtcNow,  // 自动完成的记录处理时间
                ResolvedRemark = needsManualResolution ? null : "自动记录（低级别异常）"  // 自动完成的记录备注
            };
            exceptionLog.OnCreate();
            await exceptionLogRepository.AddAsync(exceptionLog);

            // ========== 根据严重级别记录日志，待人工处理 ==========
            if (message.Severity == AgvExceptionSeverity.Error
                || message.Severity == AgvExceptionSeverity.Critical)
            {
                _logger.LogError("[MqttMessageHandler] 小车 {AgvCode} 发生严重异常: {ExceptionType}, TaskId={TaskId}, ErrorCode={ErrorCode}, 请人工处理",
                    agvCode, message.ExceptionType.ToDisplayText(), message.TaskId ?? "无", exceptionLog.ErrorCode);
            }
            else if (message.Severity == AgvExceptionSeverity.Warning)
            {
                _logger.LogWarning("[MqttMessageHandler] 小车 {AgvCode} 发生警告级异常: {ExceptionType}, ErrorCode={ErrorCode}",
                    agvCode, message.ExceptionType.ToDisplayText(), exceptionLog.ErrorCode);
            }
            else
            {
                _logger.LogInformation("[MqttMessageHandler] 小车 {AgvCode} 上报信息级异常: {ExceptionType}, ErrorCode={ErrorCode}",
                    agvCode, message.ExceptionType.ToDisplayText(), exceptionLog.ErrorCode);
            }

            _logger.LogInformation("[MqttMessageHandler] 异常日志已创建: {ExceptionType}({Severity}), ErrorCode={ErrorCode}",
                message.ExceptionType.ToDisplayText(), message.Severity.ToDisplayText(), exceptionLog.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MqttMessageHandler] 处理异常上报失败: AgvCode={AgvCode}", agvCode);
        }
    }

    /// <summary>
    /// 处理路径锁定请求消息
    /// 职责：调用路径锁定服务进行冲突检测，并发送响应消息
    /// </summary>
    public async Task HandlePathLockRequestAsync(string agvCode, PathLockRequestMessage message)
    {
        _logger.LogInformation("[MqttMessageHandler] 收到小车 {AgvCode} 的路径锁定请求: {From}→{To}, TaskId={TaskId}",
            agvCode, message.FromStationCode, message.ToStationCode, message.TaskId);

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var pathLockService = scope.ServiceProvider.GetRequiredService<IPathLockService>();
            var mqttService = scope.ServiceProvider.GetRequiredService<IMqttBrokerService>();

            // 解析任务ID
            if (!Guid.TryParse(message.TaskId, out var taskId))
            {
                _logger.LogError("[MqttMessageHandler] 无效的任务ID: {TaskId}", message.TaskId);

                // 发送拒绝响应
                var errorResponse = new PathLockResponseMessage
                {
                    TaskId = message.TaskId,
                    FromStationCode = message.FromStationCode,
                    ToStationCode = message.ToStationCode,
                    Approved = false,
                    Reason = "无效的任务ID",
                    Timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                };
                await mqttService.PublishPathLockResponseAsync(agvCode, errorResponse);
                return;
            }

            // 申请锁定
            var (approved, reason) = await pathLockService.RequestLockAsync(
                message.FromStationCode,
                message.ToStationCode,
                agvCode,
                taskId);

            // 发送响应
            var response = new PathLockResponseMessage
            {
                TaskId = message.TaskId,
                FromStationCode = message.FromStationCode,
                ToStationCode = message.ToStationCode,
                Approved = approved,
                Reason = reason,
                Timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            await mqttService.PublishPathLockResponseAsync(agvCode, response);

            if (approved)
            {
                _logger.LogInformation("[MqttMessageHandler] 批准路径锁定: {From}→{To}, AgvCode={AgvCode}, TaskId={TaskId}",
                    message.FromStationCode, message.ToStationCode, agvCode, message.TaskId);
            }
            else
            {
                _logger.LogInformation("[MqttMessageHandler] 拒绝路径锁定: {From}→{To}, AgvCode={AgvCode}, Reason={Reason}",
                    message.FromStationCode, message.ToStationCode, agvCode, reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MqttMessageHandler] 处理路径锁定请求失败: AgvCode={AgvCode}, {From}→{To}",
                agvCode, message.FromStationCode, message.ToStationCode);
        }
    }

}
