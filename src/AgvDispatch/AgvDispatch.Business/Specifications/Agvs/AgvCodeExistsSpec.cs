using AgvDispatch.Business.Entities.AgvAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Agvs;

/// <summary>
/// 检查小车编号是否存在
/// </summary>
public class AgvCodeExistsSpec : Specification<Agv>, ISingleResultSpecification<Agv>
{
    public AgvCodeExistsSpec(string agvCode)
    {
        Query.Where(x => x.AgvCode == agvCode && x.IsValid);
    }
}
