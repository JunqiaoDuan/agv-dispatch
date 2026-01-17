using AgvDispatch.Business.Entities.TaskAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 根据ID获取有效的任务
/// </summary>
public class TaskByIdSpec : Specification<TaskJob>, ISingleResultSpecification<TaskJob>
{
    public TaskByIdSpec(Guid id)
    {
        Query.Where(x => x.Id == id && x.IsValid);
    }
}
