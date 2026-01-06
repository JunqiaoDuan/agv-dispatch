using AgvDispatch.Business.Entities.AgvAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Agvs;

/// <summary>
/// 根据ID获取有效的小车
/// </summary>
public class AgvByIdSpec : Specification<Agv>, ISingleResultSpecification<Agv>
{
    public AgvByIdSpec(Guid id)
    {
        Query.Where(x => x.Id == id && x.IsValid);
    }
}
