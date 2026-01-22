using Ardalis.Specification;
using AgvDispatch.Business.Entities.BackgroundJobLogAggregate;
using AgvDispatch.Shared.DTOs.BackgroundJobLogs;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Specifications.BackgroundJobLogs;

/// <summary>
/// 后台任务日志分页查询规约
/// </summary>
public class BackgroundJobLogPagedSpec : Specification<BackgroundJobLog>
{
    public BackgroundJobLogPagedSpec(
        DateTime? startDate,
        DateTime? endDate,
        string? jobName,
        int? result,
        int pageIndex,
        int pageSize)
    {
        Query.Where(x => x.IsValid)
            .OrderByDescending(x => x.ExecuteTime);

        // 时间范围过滤
        if (startDate.HasValue)
        {
            var startDateTime = new DateTimeOffset(startDate.Value, TimeSpan.Zero);
            Query.Where(x => x.ExecuteTime >= startDateTime);
        }

        if (endDate.HasValue)
        {
            var endDateTime = new DateTimeOffset(endDate.Value.AddDays(1), TimeSpan.Zero);
            Query.Where(x => x.ExecuteTime < endDateTime);
        }

        // 任务名称过滤
        if (!string.IsNullOrWhiteSpace(jobName))
        {
            Query.Where(x => x.JobName.Contains(jobName));
        }

        // 执行结果过滤
        if (result.HasValue)
        {
            Query.Where(x => x.Result == (JobExecutionResult)result.Value);
        }

        // 分页
        Query.Skip(pageIndex * pageSize).Take(pageSize);
    }
}

/// <summary>
/// 后台任务日志计数规约（不带分页）
/// </summary>
public class BackgroundJobLogCountSpec : Specification<BackgroundJobLog>
{
    public BackgroundJobLogCountSpec(
        DateTime? startDate,
        DateTime? endDate,
        string? jobName,
        int? result)
    {
        Query.Where(x => x.IsValid);

        // 时间范围过滤
        if (startDate.HasValue)
        {
            var startDateTime = new DateTimeOffset(startDate.Value, TimeSpan.Zero);
            Query.Where(x => x.ExecuteTime >= startDateTime);
        }

        if (endDate.HasValue)
        {
            var endDateTime = new DateTimeOffset(endDate.Value.AddDays(1), TimeSpan.Zero);
            Query.Where(x => x.ExecuteTime < endDateTime);
        }

        // 任务名称过滤
        if (!string.IsNullOrWhiteSpace(jobName))
        {
            Query.Where(x => x.JobName.Contains(jobName));
        }

        // 执行结果过滤
        if (result.HasValue)
        {
            Query.Where(x => x.Result == (JobExecutionResult)result.Value);
        }
    }
}
