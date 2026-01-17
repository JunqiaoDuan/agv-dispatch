using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskRoutes;

/// <summary>
/// 根据ID获取有效的任务路线
/// </summary>
public class TaskRouteByIdSpec : Specification<TaskRoute>, ISingleResultSpecification<TaskRoute>
{
    public TaskRouteByIdSpec(Guid id)
    {
        Query.Where(x => x.Id == id && x.IsValid);
    }
}
