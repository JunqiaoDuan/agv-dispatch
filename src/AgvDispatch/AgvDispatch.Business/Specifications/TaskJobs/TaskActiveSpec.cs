using AgvDispatch.Business.Entities.TaskAggregate;
using TaskStatusEnum = AgvDispatch.Shared.Enums.TaskJobStatus;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 获取活动任务列表（待分配、已分配、执行中）
/// </summary>
public class TaskActiveSpec : Specification<TaskJob>
{
    public TaskActiveSpec()
    {
        Query.Where(x => x.IsValid &&
                        (x.TaskStatus == TaskStatusEnum.Pending ||
                         x.TaskStatus == TaskStatusEnum.Assigned ||
                         x.TaskStatus == TaskStatusEnum.Executing))
            .OrderByDescending(x => x.CreationDate);
    }
}
