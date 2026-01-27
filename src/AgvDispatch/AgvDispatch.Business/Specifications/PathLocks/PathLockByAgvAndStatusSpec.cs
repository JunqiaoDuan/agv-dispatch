using AgvDispatch.Business.Entities.PathLockAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.PathLocks;

/// <summary>
/// 按AGV和状态查询路径锁定
/// </summary>
public class PathLockByAgvAndStatusSpec : Specification<TaskPathLock>
{
    public PathLockByAgvAndStatusSpec(Guid agvId, PathLockStatus status)
    {
        Query.Where(x => x.IsValid
                         && x.LockedByAgvId == agvId
                         && x.Status == status);
    }
}
