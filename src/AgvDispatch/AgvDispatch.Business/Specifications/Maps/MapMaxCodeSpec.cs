using AgvDispatch.Business.Entities.MapAggregate;
using AgvDispatch.Shared.Constants;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Maps;

/// <summary>
/// 获取M开头的最大编号的地图
/// </summary>
public class MapMaxCodeSpec : Specification<Map>, ISingleResultSpecification<Map>
{
    public MapMaxCodeSpec()
    {
        Query.Where(x => x.MapCode.StartsWith(EntityCodes.MapPrefix))
            .OrderByDescending(x => x.MapCode);
    }
}
