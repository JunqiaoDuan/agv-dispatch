using AgvDispatch.Business.Entities.TaskAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 根据小车ID获取任务列表
/// </summary>
public class TaskByAgvIdSpec : Specification<TaskJob>
{
    public TaskByAgvIdSpec(Guid agvId)
    {
        Query.Where(x => x.AssignedAgvId == agvId && x.IsValid)
            .OrderByDescending(x => x.CreationDate);
    }
}
