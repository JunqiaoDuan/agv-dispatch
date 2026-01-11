using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapEdges;

/// <summary>
/// 检查指定节点是否被边引用
/// </summary>
public class MapEdgeByNodeIdSpec : Specification<MapEdge>
{
    public MapEdgeByNodeIdSpec(Guid nodeId)
    {
        Query.Where(x => (x.StartNodeId == nodeId || x.EndNodeId == nodeId) && x.IsValid);
    }
}
