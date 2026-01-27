using AgvDispatch.Business.Entities.PathLockAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.PathLocks;

/// <summary>
/// 按路段和AGV查询路径锁定（用于释放锁定）
/// </summary>
public class PathLockBySegmentAndAgvSpec : Specification<TaskPathLock>, ISingleResultSpecification<TaskPathLock>
{
    public PathLockBySegmentAndAgvSpec(
        string fromStationCode,
        string toStationCode,
        Guid agvId,
        PathLockStatus status)
    {
        Query.Where(x => x.IsValid
                         && x.FromStationCode == fromStationCode
                         && x.ToStationCode == toStationCode
                         && x.LockedByAgvId == agvId
                         && x.Status == status);
    }
}
