using AgvDispatch.Business.Entities.StationAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Stations;

/// <summary>
/// 根据地图ID查询站点列表
/// </summary>
public class StationsByMapIdSpec : Specification<Station>
{
    public StationsByMapIdSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid);
    }
}
