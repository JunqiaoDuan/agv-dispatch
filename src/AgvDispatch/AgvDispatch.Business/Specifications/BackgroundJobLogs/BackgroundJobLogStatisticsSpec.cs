using AgvDispatch.Business.Entities.BackgroundJobLogAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.BackgroundJobLogs;

/// <summary>
/// 后台任务日志统计规约（按时间范围）
/// </summary>
public class BackgroundJobLogStatisticsSpec : Specification<BackgroundJobLog>
{
    public BackgroundJobLogStatisticsSpec(DateTime? since)
    {
        Query.Where(x => x.IsValid);

        if (since.HasValue)
        {
            var sinceDateTime = new DateTimeOffset(since.Value, TimeSpan.Zero);
            Query.Where(x => x.ExecuteTime >= sinceDateTime);
        }
    }
}

/// <summary>
/// 后台任务日志按结果统计规约
/// </summary>
public class BackgroundJobLogByResultSpec : Specification<BackgroundJobLog>
{
    public BackgroundJobLogByResultSpec(DateTime? since, JobExecutionResult result)
    {
        Query.Where(x => x.IsValid && x.Result == result);

        if (since.HasValue)
        {
            var sinceDateTime = new DateTimeOffset(since.Value, TimeSpan.Zero);
            Query.Where(x => x.ExecuteTime >= sinceDateTime);
        }
    }
}
