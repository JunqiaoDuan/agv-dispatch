using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Routes;

/// <summary>
/// 检查指定地图中路线编号是否存在
/// </summary>
public class RouteCodeExistsSpec : Specification<Route>, ISingleResultSpecification<Route>
{
    public RouteCodeExistsSpec(Guid mapId, string routeCode)
    {
        Query.Where(x => x.MapId == mapId && x.RouteCode == routeCode && x.IsValid);
    }
}
