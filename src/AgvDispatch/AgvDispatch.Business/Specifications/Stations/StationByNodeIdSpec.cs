using AgvDispatch.Business.Entities.StationAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Stations;

/// <summary>
/// 根据节点 ID 查找站点（用于删除节点前检查）
/// </summary>
public class StationByNodeIdSpec : Specification<Station>
{
    public StationByNodeIdSpec(Guid nodeId)
    {
        Query.Where(x => x.NodeId == nodeId && x.IsValid);
    }
}
