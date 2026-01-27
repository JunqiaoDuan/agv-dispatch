using AgvDispatch.Business.Entities.TaskRouteAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.TaskRoutes;

/// <summary>
/// 根据起点和终点站点查询任务路线
/// </summary>
public class TaskRouteByStationsSpec : Specification<TaskRoute>
{
    public TaskRouteByStationsSpec(string startStationCode, string endStationCode)
    {
        Query.Where(x => x.StartStationCode == startStationCode
                      && x.EndStationCode == endStationCode
                      && x.IsValid);

        Query.OrderBy(x => x.SortNo);
    }
}
