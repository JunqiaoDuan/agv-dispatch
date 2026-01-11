using AgvDispatch.Business.Entities.StationAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Stations;

/// <summary>
/// 检查站点编号在地图中是否已存在
/// </summary>
public class StationCodeExistsSpec : Specification<Station>, ISingleResultSpecification<Station>
{
    public StationCodeExistsSpec(Guid mapId, string stationCode)
    {
        Query.Where(x => x.MapId == mapId && x.StationCode == stationCode && x.IsValid);
    }
}
