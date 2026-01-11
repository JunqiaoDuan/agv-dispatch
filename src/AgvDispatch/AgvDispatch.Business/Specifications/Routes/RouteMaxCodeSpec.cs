using AgvDispatch.Business.Entities.RouteAggregate;
using AgvDispatch.Shared.Constants;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Routes;

/// <summary>
/// 获取指定地图中R开头的最大编号的路线
/// </summary>
public class RouteMaxCodeSpec : Specification<Route>, ISingleResultSpecification<Route>
{
    public RouteMaxCodeSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.RouteCode.StartsWith(EntityCodes.RoutePrefix))
            .OrderByDescending(x => x.RouteCode);
    }
}
