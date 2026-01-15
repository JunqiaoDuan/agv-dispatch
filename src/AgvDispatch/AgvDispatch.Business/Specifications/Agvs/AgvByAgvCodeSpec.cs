using AgvDispatch.Business.Entities.AgvAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Agvs;

/// <summary>
/// 根据小车编号查询小车
/// </summary>
public class AgvByAgvCodeSpec : Specification<Agv>, ISingleResultSpecification<Agv>
{
    public AgvByAgvCodeSpec(string agvCode)
    {
        Query.Where(x => x.IsValid && x.AgvCode == agvCode);
    }
}
