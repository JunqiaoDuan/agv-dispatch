using AgvDispatch.Business.Entities.TaskAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 根据ID列表获取有效的任务
/// </summary>
public class TaskByIdsSpec : Specification<TaskJob>
{
    public TaskByIdsSpec(IEnumerable<Guid> ids)
    {
        Query.Where(x => ids.Contains(x.Id) && x.IsValid);
    }
}
