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
    /// </summary>
    public async Task HandleStatusAsync(string agvCode, StatusMessage message)
    {
        _logger.LogInformation(
            "[MqttMessageHandler] 收到小车 {AgvCode} 的状态上报: Status={Status}, Battery={Battery}%, Position=({X},{Y}), Station={Station}%,",
            agvCode, message.Status, message.Battery, message.Position.X, message.Position.Y, message.Position.StationId);

        //try
        //{
        //    // 更新数据库中的小车状态
        //    var spec = new AgvByAgvCodeSpec(agvCode);
        //    var agv = await _agvRepository.FirstOrDefaultAsync(spec);

        //    if (agv != null)
        //    {
        //        agv.AgvStatus = message.Status;
        //        agv.Battery = message.Battery;
        //        agv.Speed = (decimal)message.Speed;
        //        agv.PositionX = message.Position.X.HasValue ? (decimal)message.Position.X.Value : agv.PositionX;
        //        agv.PositionY = message.Position.Y.HasValue ? (decimal)message.Position.Y.Value : agv.PositionY;
        //        agv.PositionAngle = message.Position.Angle.HasValue ? (decimal)message.Position.Angle.Value : agv.PositionAngle;
        //        agv.ErrorCode = message.ErrorCode;
        //        agv.LastOnlineTime = DateTimeOffset.UtcNow;

        //        await _agvRepository.UpdateAsync(agv);

        //        _logger.LogDebug("[MqttMessageHandler] 小车 {AgvCode} 状态已更新", agvCode);
        //    }
        //    else
        //    {
        //        _logger.LogError("[MqttMessageHandler] 小车 {AgvCode} 不存在，无法更新状态", agvCode);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "[MqttMessageHandler] 更新小车 {AgvCode} 状态失败", agvCode);
        //}
    }

    /// <summary>
    /// 处理任务进度消息
    /// </summary>
    public async Task HandleTaskProgressAsync(string agvCode, TaskProgressMessage message)
    {
        _logger.LogInformation("[MqttMessageHandler] 收到小车 {AgvCode} 的任务进度: TaskId={TaskId}, Status={Status}, Progress={Progress}%, Message={Message}",
            agvCode, message.TaskId, message.Status, message.ProgressPercentage, message.Message);

        // TODO: 实现任务进度更新
        // 需要创建 Task 实体和 IRepository<AgvTask>
        // var taskSpec = new TaskByIdSpec(message.TaskId);
        // var task = await _taskRepository.FirstOrDefaultAsync(taskSpec);
        // if (task != null)
        // {
        //     task.Status = message.Status;
        //     task.ProgressPercentage = message.ProgressPercentage;
        //     if (message.Status == TaskStatus.Completed)
        //     {
        //         task.CompletedAt = DateTimeOffset.UtcNow;
        //     }
        //     await _taskRepository.UpdateAsync(task);
        // }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 处理异常上报消息
    /// </summary>
    public async Task HandleExceptionAsync(string agvCode, ExceptionMessage message)
    {
        _logger.LogWarning("[MqttMessageHandler] 收到小车 {AgvCode} 的异常上报: Type={Type}, Severity={Severity}, Message={Message}",
            agvCode, message.ExceptionType, message.Severity, message.Message);

        // TODO: 实现异常记录保存
        // 需要创建 ExceptionLog 实体和 IRepository<ExceptionLog>
        // var exceptionLog = new ExceptionLog
        // {
        //     AgvCode = agvCode,
        //     TaskId = message.TaskId,
        //     ExceptionType = message.ExceptionType,
        //     Severity = message.Severity,
        //     Message = message.Message,
        //     PositionX = message.Position?.X,
        //     PositionY = message.Position?.Y
        // };
        // exceptionLog.OnCreate();
        // await _exceptionLogRepository.AddAsync(exceptionLog);

        await Task.CompletedTask;
    }

}
