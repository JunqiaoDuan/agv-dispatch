using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapNodes;

/// <summary>
/// 根据ID获取有效的节点
/// </summary>
public class MapNodeByIdSpec : Specification<MapNode>, ISingleResultSpecification<MapNode>
{
    public MapNodeByIdSpec(Guid id)
    {
        Query.Where(x => x.Id == id && x.IsValid);
    }
}
