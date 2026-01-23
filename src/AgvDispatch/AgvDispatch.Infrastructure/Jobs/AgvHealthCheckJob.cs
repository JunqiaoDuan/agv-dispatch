using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.BackgroundJobLogAggregate;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Business.Services;
using AgvDispatch.Business.Specifications.TaskJobs;
using AgvDispatch.Infrastructure.Options;
using AgvDispatch.Shared.DTOs.BackgroundJobLogs;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Messages;
using AgvDispatch.Shared.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.Diagnostics;
using System.Text.Json;

namespace AgvDispatch.Infrastructure.Jobs;

/// <summary>
/// AGV 健康检测定时任务
/// 检测长时间未发送消息的小车并标记为离线
/// </summary>
[DisallowConcurrentExecution]  // 防止同一任务并发执行
public class AgvHealthCheckJob : IJob
{
    private readonly ILogger<AgvHealthCheckJob> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly HealthCheckOptions _options;

    public AgvHealthCheckJob(
        ILogger<AgvHealthCheckJob> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<HealthCheckOptions> options)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var executeTime = DateTimeOffset.UtcNow;

        _logger.LogDebug("[AgvHealthCheckJob] 开始执行 AGV 健康检测");

        BackgroundJobLog? jobLog = null;
        List<string> offlineAgvCodes = new();

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var agvRepository = scope.ServiceProvider.GetRequiredService<IRepository<Agv>>();
            var taskRepository = scope.ServiceProvider.GetRequiredService<IRepository<TaskJob>>();
            var mqttBrokerService = scope.ServiceProvider.GetRequiredService<IMqttBrokerService>();
            var jobLogRepository = scope.ServiceProvider.GetRequiredService<IRepository<BackgroundJobLog>>();

            // 获取所有非离线状态的小车
            var allAgvs = await agvRepository.ListAsync();
            var onlineAgvs = allAgvs.Where(a => a.AgvStatus != AgvStatus.Offline).ToList();

            if (onlineAgvs.Count == 0)
            {
                _logger.LogDebug("[AgvHealthCheckJob] 没有在线的小车需要检测");
                return;
            }

            // 计算离线时间阈值
            var offlineThreshold = DateTimeOffset.UtcNow.AddSeconds(-_options.AgvOfflineThresholdSeconds);

            // 查找超时的小车
            var offlineAgvs = onlineAgvs
                .Where(a => a.LastOnlineTime == null || a.LastOnlineTime.Value < offlineThreshold)
                .ToList();

            if (offlineAgvs.Count == 0)
            {
                _logger.LogDebug("[AgvHealthCheckJob] 所有在线小车状态正常");
                return;
            }

            // 批量更新为离线状态
            foreach (var agv in offlineAgvs)
            {
                var lastOnlineTimeText = agv.LastOnlineTime.HasValue
                    ? agv.LastOnlineTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : "从未上线";

                _logger.LogWarning(
                    "[AgvHealthCheckJob] 小车 {AgvCode} 超过 {Threshold}秒未发送消息，标记为离线。最后在线时间：{LastOnlineTime}",
                    agv.AgvCode, _options.AgvOfflineThresholdSeconds, lastOnlineTimeText);

                agv.AgvStatus = AgvStatus.Offline;
                await agvRepository.UpdateAsync(agv);
                offlineAgvCodes.Add(agv.AgvCode);

                // 检查并停止该 AGV 正在运行的任务
                var runningTasksSpec = new TaskRunningByAgvCodeSpec(agv.AgvCode);
                var runningTasks = await taskRepository.ListAsync(runningTasksSpec);

                if (runningTasks.Any())
                {
                    foreach (var task in runningTasks)
                    {
                        task.TaskStatus = TaskJobStatus.Cancelled;
                        task.CancelledAt = DateTimeOffset.UtcNow;
                        task.CancelReason = $"AGV 离线（超过 {_options.AgvOfflineThresholdSeconds} 秒未发送消息）";
                        await taskRepository.UpdateAsync(task);

                        // 发送 MQTT 取消消息
                        var cancelMessage = new TaskCancelMessage
                        {
                            TaskId = task.Id.ToString(),
                            Timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            Reason = task.CancelReason
                        };
                        await mqttBrokerService.PublishTaskCancelAsync(agv.AgvCode, cancelMessage);

                        _logger.LogWarning(
                            "[AgvHealthCheckJob] 已取消任务 {TaskId}（AGV {AgvCode} 离线）",
                            task.Id, agv.AgvCode);
                    }
                }
            }

            _logger.LogInformation(
                "[AgvHealthCheckJob] 本次检测完成，共标记 {Count} 台小车为离线",
                offlineAgvs.Count);

            // 记录成功日志
            stopwatch.Stop();

            var details = JsonSerializer.Serialize(new
            {
                OfflineAgvCodes = offlineAgvCodes,
                ThresholdSeconds = _options.AgvOfflineThresholdSeconds
            });

            jobLog = CreateJobLog(
                executeTime,
                JobExecutionResult.Success,
                $"标记 {offlineAgvCodes.Count} 台小车为离线",
                details,
                offlineAgvCodes.Count,
                stopwatch.ElapsedMilliseconds);

            jobLog.OnCreate();
            await jobLogRepository.AddAsync(jobLog);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[AgvHealthCheckJob] 执行 AGV 健康检测失败");

            // 记录失败日志
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var jobLogRepository = scope.ServiceProvider.GetRequiredService<IRepository<BackgroundJobLog>>();

                jobLog = CreateJobLog(
                    executeTime,
                    JobExecutionResult.Failed,
                    "执行失败",
                    null,
                    0,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);

                jobLog.OnCreate();
                await jobLogRepository.AddAsync(jobLog);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "[AgvHealthCheckJob] 记录任务日志失败");
            }
        }
    }

    private BackgroundJobLog CreateJobLog(
        DateTimeOffset executeTime,
        JobExecutionResult result,
        string message,
        string? details,
        int affectedCount,
        long durationMs,
        string? errorMessage = null)
    {
        return new BackgroundJobLog
        {
            JobName = nameof(AgvHealthCheckJob),
            JobDisplayName = "AGV 健康检测",
            ExecuteTime = executeTime,
            Result = result,
            Message = message,
            Details = details,
            AffectedCount = affectedCount,
            DurationMs = durationMs,
            ErrorMessage = errorMessage
        };
    }
}
