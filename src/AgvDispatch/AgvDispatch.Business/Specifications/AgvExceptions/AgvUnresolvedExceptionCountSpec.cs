using AgvDispatch.Business.Entities.AgvAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.AgvExceptions;

/// <summary>
/// 查询指定AGV未解决的异常数量
/// </summary>
public class AgvUnresolvedExceptionCountSpec : Specification<AgvExceptionLog>
{
    public AgvUnresolvedExceptionCountSpec(string agvCode)
    {
        Query.Where(x => x.AgvCode == agvCode && !x.IsResolved && x.IsValid);
    }
}
