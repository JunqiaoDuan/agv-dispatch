using AgvDispatch.Business.Entities.AgvAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.AgvExceptions;

/// <summary>
/// 查询指定AGV所有未解决的异常
/// </summary>
public class AgvAllUnresolvedExceptionsSpec : Specification<AgvExceptionLog>
{
    public AgvAllUnresolvedExceptionsSpec(string agvCode)
    {
        Query
            .Where(x => x.AgvCode == agvCode && !x.IsResolved && x.IsValid)
            .OrderByDescending(x => x.ExceptionTime);
    }
}
