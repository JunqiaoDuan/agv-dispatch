using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Routes;

/// <summary>
/// 检查指定地图是否有关联的路线
/// </summary>
public class RouteByMapIdSpec : Specification<Route>
{
    public RouteByMapIdSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid);
    }
}
