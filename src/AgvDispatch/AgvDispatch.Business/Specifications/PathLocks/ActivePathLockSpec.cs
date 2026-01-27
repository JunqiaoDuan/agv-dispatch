using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.PathLocks;

/// <summary>
/// 查询活跃的路径锁定（Approved状态）
/// </summary>
public class ActivePathLockSpec : Specification<TaskPathLock>, ISingleResultSpecification<TaskPathLock>
{
    public ActivePathLockSpec(string fromStationCode, string toStationCode)
    {
        Query.Where(x => x.IsValid
                         && x.FromStationCode == fromStationCode
                         && x.ToStationCode == toStationCode
                         && x.Status == PathLockStatus.Approved);
    }
}
