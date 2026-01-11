using AgvDispatch.Business.Entities.StationAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Stations;

/// <summary>
/// 统计地图中的站点数量
/// </summary>
public class StationCountSpec : Specification<Station>
{
    public StationCountSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid);
    }
}
