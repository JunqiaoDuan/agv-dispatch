using AgvDispatch.Business.Entities.TaskAggregate;
using TaskStatusEnum = AgvDispatch.Shared.Enums.TaskJobStatus;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 根据任务状态获取任务列表
/// </summary>
public class TaskByStatusSpec : Specification<TaskJob>
{
    public TaskByStatusSpec(TaskStatusEnum status)
    {
        Query.Where(x => x.TaskStatus == status && x.IsValid)
            .OrderByDescending(x => x.CreationDate);
    }
}
