using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 获取所有正在运行的任务（已分配、执行中）
/// </summary>
public class TaskRunningSpec : Specification<TaskJob>
{
    public TaskRunningSpec()
    {
        Query.Where(x => x.IsValid
                      && (x.TaskStatus == TaskJobStatus.Assigned || x.TaskStatus == TaskJobStatus.Executing))
            .OrderByDescending(x => x.CreationDate);
    }
}
