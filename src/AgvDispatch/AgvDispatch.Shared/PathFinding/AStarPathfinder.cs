using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.DTOs.Stations;

namespace AgvDispatch.Shared.PathFinding;

/// <summary>
/// A* 路径查找算法实现
/// </summary>
public class AStarPathfinder
{
    private readonly List<MapNodeListItemDto> _nodes;
    private readonly List<MapEdgeListItemDto> _edges;
    private readonly List<StationListItemDto> _stations;
    private readonly Dictionary<Guid, MapNodeListItemDto> _nodeDict;
    private readonly Dictionary<Guid, List<(Guid edgeId, Guid targetNodeId, decimal distance)>> _adjacencyList;

    public AStarPathfinder(
        List<MapNodeListItemDto> nodes,
        List<MapEdgeListItemDto> edges,
        List<StationListItemDto> stations)
    {
        _nodes = nodes;
        _edges = edges;
        _stations = stations;

        // 构建节点字典
        _nodeDict = nodes.ToDictionary(n => n.Id);

        // 构建邻接表（支持单向和双向边）
        _adjacencyList = new Dictionary<Guid, List<(Guid, Guid, decimal)>>();
        foreach (var node in nodes)
        {
            _adjacencyList[node.Id] = [];
        }

        foreach (var edge in edges)
        {
            // 正向边
            _adjacencyList[edge.StartNodeId].Add((edge.Id, edge.EndNodeId, edge.Distance));

            // 如果是双向边，添加反向边
            if (edge.IsBidirectional)
            {
                _adjacencyList[edge.EndNodeId].Add((edge.Id, edge.StartNodeId, edge.Distance));
            }
        }
    }

    /// <summary>
    /// 查找从起点到终点的最短路径
    /// </summary>
    /// <param name="startId">起点ID（可以是节点或站点）</param>
    /// <param name="endId">终点ID（可以是节点或站点）</param>
    public PathfindingResult FindPath(Guid startId, Guid endId)
    {
        // 解析起点和终点（支持站点自动解析为关联节点）
        var startNodeId = ResolveToNodeId(startId);
        var endNodeId = ResolveToNodeId(endId);

        if (startNodeId == null)
            return PathfindingResult.CreateFailure("起点不存在或未关联节点");

        if (endNodeId == null)
            return PathfindingResult.CreateFailure("终点不存在或未关联节点");

        if (startNodeId == endNodeId)
            return PathfindingResult.CreateFailure("起点和终点相同");

        if (!_nodeDict.ContainsKey(startNodeId.Value))
            return PathfindingResult.CreateFailure("起点节点不存在");

        if (!_nodeDict.ContainsKey(endNodeId.Value))
            return PathfindingResult.CreateFailure("终点节点不存在");

        // 执行 A* 搜索
        return AStarSearch(startNodeId.Value, endNodeId.Value);
    }

    /// <summary>
    /// 解析ID为节点ID（站点自动解析为关联节点）
    /// </summary>
    private Guid? ResolveToNodeId(Guid id)
    {
        // 先检查是否是节点
        if (_nodeDict.ContainsKey(id))
            return id;

        // 检查是否是站点
        var station = _stations.FirstOrDefault(s => s.Id == id);
        if (station != null)
            return station.NodeId;

        return null;
    }

    /// <summary>
    /// A* 搜索算法核心实现
    /// </summary>
    private PathfindingResult AStarSearch(Guid startNodeId, Guid endNodeId)
    {
        var startNode = _nodeDict[startNodeId];
        var endNode = _nodeDict[endNodeId];

        // 优先队列（按 FCost 排序）
        var openSet = new PriorityQueue<Guid, decimal>();
        var cameFrom = new Dictionary<Guid, (Guid nodeId, Guid edgeId)>();
        var gCost = new Dictionary<Guid, decimal> { [startNodeId] = 0 };
        var fCost = new Dictionary<Guid, decimal> { [startNodeId] = CalculateHeuristic(startNode, endNode) };

        openSet.Enqueue(startNodeId, fCost[startNodeId]);
        var closedSet = new HashSet<Guid>();

        while (openSet.Count > 0)
        {
            var currentNodeId = openSet.Dequeue();

            // 找到终点
            if (currentNodeId == endNodeId)
            {
                return ReconstructPath(cameFrom, endNodeId, gCost[endNodeId]);
            }

            closedSet.Add(currentNodeId);

            // 遍历邻居
            if (!_adjacencyList.ContainsKey(currentNodeId))
                continue;

            foreach (var (edgeId, neighborId, distance) in _adjacencyList[currentNodeId])
            {
                if (closedSet.Contains(neighborId))
                    continue;

                var tentativeGCost = gCost[currentNodeId] + distance;

                if (!gCost.ContainsKey(neighborId) || tentativeGCost < gCost[neighborId])
                {
                    cameFrom[neighborId] = (currentNodeId, edgeId);
                    gCost[neighborId] = tentativeGCost;

                    var neighborNode = _nodeDict[neighborId];
                    var hCost = CalculateHeuristic(neighborNode, endNode);
                    fCost[neighborId] = tentativeGCost + hCost;

                    openSet.Enqueue(neighborId, fCost[neighborId]);
                }
            }
        }

        return PathfindingResult.CreateFailure("未找到可达路径");
    }

    /// <summary>
    /// 计算启发式距离（欧几里得距离）
    /// </summary>
    private decimal CalculateHeuristic(MapNodeListItemDto from, MapNodeListItemDto to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        return (decimal)Math.Sqrt((double)(dx * dx + dy * dy));
    }

    /// <summary>
    /// 重建路径
    /// </summary>
    private PathfindingResult ReconstructPath(
        Dictionary<Guid, (Guid nodeId, Guid edgeId)> cameFrom,
        Guid endNodeId,
        decimal totalDistance)
    {
        var nodePath = new List<Guid>();
        var edgePath = new List<Guid>();

        var currentNodeId = endNodeId;
        nodePath.Add(currentNodeId);

        while (cameFrom.ContainsKey(currentNodeId))
        {
            var (prevNodeId, edgeId) = cameFrom[currentNodeId];
            edgePath.Add(edgeId);
            nodePath.Add(prevNodeId);
            currentNodeId = prevNodeId;
        }

        nodePath.Reverse();
        edgePath.Reverse();

        return PathfindingResult.CreateSuccess(nodePath, edgePath, totalDistance);
    }
}
