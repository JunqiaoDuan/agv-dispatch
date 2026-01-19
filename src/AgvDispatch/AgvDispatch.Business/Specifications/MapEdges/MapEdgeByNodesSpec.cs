using AgvDispatch.Business.Entities.MapAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapEdges;

/// <summary>
/// 根据起始和结束节点查询边
/// </summary>
public class MapEdgeByNodesSpec : Specification<MapEdge>
{
    public MapEdgeByNodesSpec(Guid fromNodeId, Guid toNodeId)
    {
        Query.Where(x => x.IsValid &&
            ((x.StartNodeId == fromNodeId && x.EndNodeId == toNodeId) ||
             (x.IsBidirectional && x.StartNodeId == toNodeId && x.EndNodeId == fromNodeId)));
    }
}
