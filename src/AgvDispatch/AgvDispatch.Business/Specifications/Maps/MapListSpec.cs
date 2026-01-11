using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Maps;

/// <summary>
/// 获取有效的地图列表
/// </summary>
public class MapListSpec : Specification<Map>
{
    public MapListSpec()
    {
        Query.Where(x => x.IsValid)
            .OrderBy(x => x.SortNo)
            .ThenBy(x => x.MapCode);
    }
}
