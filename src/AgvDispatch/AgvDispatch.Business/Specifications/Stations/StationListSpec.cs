using AgvDispatch.Business.Entities.StationAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Stations;

/// <summary>
/// 获取地图的所有有效站点列表
/// </summary>
public class StationListSpec : Specification<Station>
{
    public StationListSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid)
            .OrderBy(x => x.SortNo)
            .ThenBy(x => x.StationCode);
    }
}
