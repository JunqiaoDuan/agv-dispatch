using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 根据小车ID获取所有占用的路段(只查询已批准的锁定)
/// </summary>
public class TaskPathLockApprovedByAgvIdSpec : Specification<TaskPathLock>
{
    public TaskPathLockApprovedByAgvIdSpec(Guid agvId)
    {
        Query.Where(x => x.LockedByAgvId == agvId
                      && x.Status == PathLockStatus.Approved
                      && x.IsValid);
    }
}
