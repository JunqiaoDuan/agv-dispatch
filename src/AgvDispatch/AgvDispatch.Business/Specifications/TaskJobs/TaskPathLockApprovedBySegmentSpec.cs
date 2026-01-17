using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskJobs;

/// <summary>
/// 根据路段查询锁定记录(只查询已批准的锁定)
/// </summary>
public class TaskPathLockApprovedBySegmentSpec : Specification<TaskPathLock>, ISingleResultSpecification<TaskPathLock>
{
    public TaskPathLockApprovedBySegmentSpec(string fromStationCode, string toStationCode)
    {
        Query.Where(x => x.FromStationCode == fromStationCode
                      && x.ToStationCode == toStationCode
                      && x.Status == PathLockStatus.Approved
                      && x.IsValid);
    }
}
