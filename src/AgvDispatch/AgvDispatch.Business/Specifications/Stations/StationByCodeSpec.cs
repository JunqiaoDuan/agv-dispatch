using AgvDispatch.Business.Entities.StationAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Stations;

/// <summary>
/// 根据站点编号查询站点
/// </summary>
public class StationByCodeSpec : Specification<Station>, ISingleResultSpecification<Station>
{
    public StationByCodeSpec(string stationCode)
    {
        Query.Where(x => x.StationCode == stationCode && x.IsValid);
    }
}
