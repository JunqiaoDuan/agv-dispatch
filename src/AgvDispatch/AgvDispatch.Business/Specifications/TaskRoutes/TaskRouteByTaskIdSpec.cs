using AgvDispatch.Business.Entities.RouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskRoutes;

/// <summary>
/// 根据任务ID获取有效的任务路线
/// </summary>
public class TaskRouteByTaskIdSpec : Specification<TaskRoute>, ISingleResultSpecification<TaskRoute>
{
    public TaskRouteByTaskIdSpec(Guid taskId)
    {
        Query.Where(x => x.TaskId == taskId && x.IsValid);
    }
}
