using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapNodes;

/// <summary>
/// 检查指定地图中节点编号是否存在
/// </summary>
public class MapNodeCodeExistsSpec : Specification<MapNode>, ISingleResultSpecification<MapNode>
{
    public MapNodeCodeExistsSpec(Guid mapId, string nodeCode)
    {
        Query.Where(x => x.MapId == mapId && x.NodeCode == nodeCode && x.IsValid);
    }
}
