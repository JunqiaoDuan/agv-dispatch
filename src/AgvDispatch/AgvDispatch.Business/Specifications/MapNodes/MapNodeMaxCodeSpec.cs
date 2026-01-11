using AgvDispatch.Business.Entities.MapAggregate;
using AgvDispatch.Shared.Constants;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.MapNodes;

/// <summary>
/// 获取指定地图中N开头的最大编号的节点
/// </summary>
public class MapNodeMaxCodeSpec : Specification<MapNode>, ISingleResultSpecification<MapNode>
{
    public MapNodeMaxCodeSpec(Guid mapId)
    {
        Query.Where(x => x.MapId == mapId && x.NodeCode.StartsWith(EntityCodes.NodePrefix))
            .OrderByDescending(x => x.NodeCode);
    }
}
