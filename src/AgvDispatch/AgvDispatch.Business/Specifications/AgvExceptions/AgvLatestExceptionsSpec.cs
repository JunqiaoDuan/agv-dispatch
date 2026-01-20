using AgvDispatch.Business.Entities.AgvAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.AgvExceptions;

/// <summary>
/// 查询指定AGV最新的未解决异常
/// </summary>
public class AgvLatestExceptionsSpec : Specification<AgvExceptionLog>
{
    public AgvLatestExceptionsSpec(string agvCode, int limit)
    {
        Query
            .Where(x => x.AgvCode == agvCode && !x.IsResolved && x.IsValid)
            .OrderByDescending(x => x.ExceptionTime)
            .Take(limit);
    }
}
