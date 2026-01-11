using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Maps;

/// <summary>
/// 根据ID获取有效的地图
/// </summary>
public class MapByIdSpec : Specification<Map>, ISingleResultSpecification<Map>
{
    public MapByIdSpec(Guid id)
    {
        Query.Where(x => x.Id == id && x.IsValid);
    }
}
