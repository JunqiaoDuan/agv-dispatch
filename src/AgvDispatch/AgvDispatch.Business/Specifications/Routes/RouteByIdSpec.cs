using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Routes;

/// <summary>
/// 根据ID获取有效的路线
/// </summary>
public class RouteByIdSpec : Specification<Route>, ISingleResultSpecification<Route>
{
    public RouteByIdSpec(Guid id)
    {
        Query.Where(x => x.Id == id && x.IsValid);
    }
}
