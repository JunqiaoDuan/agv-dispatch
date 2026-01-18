using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.PathSegmentLockConfigs;

/// <summary>
/// 根据路段查询锁定配置
/// </summary>
public class PathSegmentLockConfigBySegmentSpec : Specification<PathSegmentLockConfig>, ISingleResultSpecification<PathSegmentLockConfig>
{
    public PathSegmentLockConfigBySegmentSpec(string fromStationCode, string toStationCode)
    {
        Query.Where(x => x.FromStationCode == fromStationCode
                      && x.ToStationCode == toStationCode
                      && x.IsValid);
    }
}
