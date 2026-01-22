using AgvDispatch.Business.Entities.TaskAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 根据小车编号获取任务列表
/// </summary>
public class TaskByAgvCodeSpec : Specification<TaskJob>
{
    public TaskByAgvCodeSpec(string agvCode)
    {
        Query.Where(x => x.AssignedAgvCode == agvCode && x.IsValid)
            .OrderByDescending(x => x.CreationDate);
    }
}
