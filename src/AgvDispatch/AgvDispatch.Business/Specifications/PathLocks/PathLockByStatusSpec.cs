using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.PathLocks;

/// <summary>
/// 查询指定状态的路径锁定
/// </summary>
public class PathLockByStatusSpec : Specification<TaskPathLock>
{
    public PathLockByStatusSpec(PathLockStatus status)
    {
        Query.Where(x => x.IsValid && x.Status == status);
    }
}
