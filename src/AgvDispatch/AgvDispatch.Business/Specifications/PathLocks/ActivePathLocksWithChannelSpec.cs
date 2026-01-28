using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.PathLocks;

/// <summary>
/// 查询所有 Approved 状态且有 ChannelName 的路径锁定
/// </summary>
public class ActivePathLocksWithChannelSpec : Specification<TaskPathLock>
{
    public ActivePathLocksWithChannelSpec()
    {
        Query.Where(x => x.IsValid
                         && x.Status == PathLockStatus.Approved
                         && x.ChannelName != null
                         && x.ChannelName != string.Empty);
    }
}
