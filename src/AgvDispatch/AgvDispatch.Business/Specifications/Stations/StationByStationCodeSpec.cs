using AgvDispatch.Business.Entities.StationAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Stations;

/// <summary>
/// 根据站点编号获取站点
/// </summary>
public class StationByStationCodeSpec : Specification<Station>, ISingleResultSpecification<Station>
{
    public StationByStationCodeSpec(string stationCode)
    {
        Query.Where(x => x.StationCode == stationCode && x.IsValid);
    }
}
