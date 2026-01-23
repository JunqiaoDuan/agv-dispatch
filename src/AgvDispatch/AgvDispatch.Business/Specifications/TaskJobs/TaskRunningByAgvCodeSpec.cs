using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 根据小车编号获取正在运行的任务（已分配、执行中）
/// </summary>
public class TaskRunningByAgvCodeSpec : Specification<TaskJob>
{
    public TaskRunningByAgvCodeSpec(string agvCode)
    {
        Query.Where(x => x.AssignedAgvCode == agvCode
                      && x.IsValid
                      && (x.TaskStatus == TaskJobStatus.Assigned || x.TaskStatus == TaskJobStatus.Executing))
            .OrderByDescending(x => x.CreationDate);
    }
}
