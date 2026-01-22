using Ardalis.Specification;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 任务分页查询规约
/// </summary>
public class TaskJobPagedSpec : Specification<TaskJob>
{
    public TaskJobPagedSpec(
        DateTime? startDate,
        DateTime? endDate,
        string? agvCode,
        TaskJobStatus? status,
        TaskJobType? taskType,
        int pageIndex,
        int pageSize)
    {
        Query.Where(x => x.IsValid)
            .OrderByDescending(x => x.CreationDate);

        // 时间范围过滤
        if (startDate.HasValue)
        {
            var startDateTime = new DateTimeOffset(startDate.Value, TimeSpan.Zero);
            Query.Where(x => x.CreationDate >= startDateTime);
        }

        if (endDate.HasValue)
        {
            var endDateTime = new DateTimeOffset(endDate.Value.AddDays(1).AddSeconds(-1), TimeSpan.Zero);
            Query.Where(x => x.CreationDate <= endDateTime);
        }

        // AGV编号过滤
        if (!string.IsNullOrWhiteSpace(agvCode))
        {
            Query.Where(x => x.AssignedAgvCode == agvCode);
        }

        // 任务状态过滤
        if (status.HasValue)
        {
            Query.Where(x => x.TaskStatus == status.Value);
        }

        // 任务类型过滤
        if (taskType.HasValue)
        {
            Query.Where(x => x.TaskType == taskType.Value);
        }

        // 分页
        Query.Skip(pageIndex * pageSize).Take(pageSize);
    }
}

/// <summary>
/// 任务计数规约（不带分页）
/// </summary>
public class TaskJobCountSpec : Specification<TaskJob>
{
    public TaskJobCountSpec(
        DateTime? startDate,
        DateTime? endDate,
        string? agvCode,
        TaskJobStatus? status,
        TaskJobType? taskType)
    {
        Query.Where(x => x.IsValid);

        // 时间范围过滤
        if (startDate.HasValue)
        {
            var startDateTime = new DateTimeOffset(startDate.Value, TimeSpan.Zero);
            Query.Where(x => x.CreationDate >= startDateTime);
        }

        if (endDate.HasValue)
        {
            var endDateTime = new DateTimeOffset(endDate.Value.AddDays(1).AddSeconds(-1), TimeSpan.Zero);
            Query.Where(x => x.CreationDate <= endDateTime);
        }

        // AGV编号过滤
        if (!string.IsNullOrWhiteSpace(agvCode))
        {
            Query.Where(x => x.AssignedAgvCode == agvCode);
        }

        // 任务状态过滤
        if (status.HasValue)
        {
            Query.Where(x => x.TaskStatus == status.Value);
        }

        // 任务类型过滤
        if (taskType.HasValue)
        {
            Query.Where(x => x.TaskType == taskType.Value);
        }
    }
}
