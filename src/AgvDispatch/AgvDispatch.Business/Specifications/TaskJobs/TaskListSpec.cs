using AgvDispatch.Business.Entities.TaskAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 获取有效的任务列表
/// </summary>
public class TaskListSpec : Specification<TaskJob>
{
    public TaskListSpec()
    {
        Query.Where(x => x.IsValid)
            .OrderByDescending(x => x.CreationDate);
    }
}
