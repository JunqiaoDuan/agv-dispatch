using AgvDispatch.Business.Entities.StationAggregate;
using AgvDispatch.Shared.Constants;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Stations;

/// <summary>
/// 获取地图中S开头的最大编号的站点
/// </summary>
public class StationMaxCodeSpec : Specification<Station>, ISingleResultSpecification<Station>
{
    public StationMaxCodeSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.StationCode.StartsWith(EntityCodes.StationPrefix))
            .OrderByDescending(x => x.StationCode);
    }
}
