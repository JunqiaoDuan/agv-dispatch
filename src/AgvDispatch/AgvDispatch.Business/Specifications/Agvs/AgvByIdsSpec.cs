using AgvDispatch.Business.Entities.AgvAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Agvs;

/// <summary>
/// 根据ID列表获取有效的AGV
/// </summary>
public class AgvByIdsSpec : Specification<Agv>
{
    public AgvByIdsSpec(IEnumerable<Guid> ids)
    {
        Query.Where(x => ids.Contains(x.Id) && x.IsValid);
    }
}
