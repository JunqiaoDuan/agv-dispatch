using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.PathSegmentLockConfigs;

/// <summary>
/// 根据ID获取路段锁定配置
/// </summary>
public class PathSegmentLockConfigByIdSpec : Specification<PathSegmentLockConfig>, ISingleResultSpecification<PathSegmentLockConfig>
{
    public PathSegmentLockConfigByIdSpec(Guid id)
    {
        Query.Where(x => x.Id == id && x.IsValid);
    }
}
