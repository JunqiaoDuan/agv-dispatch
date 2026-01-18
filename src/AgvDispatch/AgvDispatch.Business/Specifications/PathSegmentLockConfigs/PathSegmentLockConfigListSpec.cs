using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.PathSegmentLockConfigs;

/// <summary>
/// 获取所有有效路段锁定配置列表
/// </summary>
public class PathSegmentLockConfigListSpec : Specification<PathSegmentLockConfig>
{
    public PathSegmentLockConfigListSpec()
    {
        Query.Where(x => x.IsValid)
            .OrderBy(x => x.FromStationCode)
            .ThenBy(x => x.ToStationCode);
    }
}
