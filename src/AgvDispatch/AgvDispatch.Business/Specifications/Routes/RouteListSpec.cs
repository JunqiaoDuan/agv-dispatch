using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Routes;

/// <summary>
/// 获取指定地图的有效路线列表
/// </summary>
public class RouteListSpec : Specification<Route>
{
    public RouteListSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.IsValid)
            .OrderBy(x => x.SortNo)
            .ThenBy(x => x.RouteCode);
    }
}
