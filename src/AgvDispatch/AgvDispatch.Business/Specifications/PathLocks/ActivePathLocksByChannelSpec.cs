using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.PathLocks;

/// <summary>
/// 根据通道名称查询已批准的路径锁定
/// </summary>
public class ActivePathLocksByChannelSpec : Specification<TaskPathLock>
{
    public ActivePathLocksByChannelSpec(string channelName)
    {
        Query.Where(x => x.IsValid
                         && x.Status == PathLockStatus.Approved
                         && x.ChannelName == channelName)
             .OrderByDescending(x => x.RequestTime);
    }
}
