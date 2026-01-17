using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 获取已过期的路径锁定记录(只查询已批准的锁定)
/// </summary>
public class TaskPathLockApprovedExpiredSpec : Specification<TaskPathLock>
{
    public TaskPathLockApprovedExpiredSpec()
    {
        var now = DateTimeOffset.UtcNow;
        Query.Where(x => x.ExpireAt != null
                      && x.ExpireAt < now
                      && x.Status == PathLockStatus.Approved
                      && x.IsValid);
    }
}
