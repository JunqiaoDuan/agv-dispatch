using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Maps;

/// <summary>
/// 检查地图编号是否存在
/// </summary>
public class MapCodeExistsSpec : Specification<Map>, ISingleResultSpecification<Map>
{
    public MapCodeExistsSpec(string mapCode)
    {
        Query.Where(x => x.MapCode == mapCode && x.IsValid);
    }
}
