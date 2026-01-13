using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;

namespace AgvDispatch.Shared.Rendering;

/// <summary>
/// 地图验证器 - 验证地图配置是否符合 AGV 调度要求
/// </summary>
public class MapValidator
{
    /// <summary>
    /// 验证地图
    /// </summary>
    public MapValidationResult Validate(List<MapNodeListItemDto> nodes, List<MapEdgeListItemDto> edges)
    {
        var result = new MapValidationResult
        {
            NodeCount = nodes.Count,
            EdgeCount = edges.Count
        };

        // 错误级别验证
        ValidateNoNodes(nodes, result);
        ValidateNoEdges(edges, result);
        ValidateIsolatedNodes(nodes, edges, result);
        ValidateSelfLoopEdges(edges, result);
        ValidateDuplicateEdges(edges, result);

        // 警告级别验证
        ValidateDeadEndNoExit(nodes, edges, result);
        ValidateDeadEndNoEntry(nodes, edges, result);
        ValidateUnreachableNodes(nodes, edges, result);
        ValidateNoReturnNodes(nodes, edges, result);

        // 设置连通性状态
        result.ConnectivityStatus = result.Errors.Count == 0 && result.Warnings.Count == 0
            ? "良好"
            : result.Errors.Count == 0 ? "存在警告" : "存在问题";

        return result;
    }

    private void ValidateNoNodes(List<MapNodeListItemDto> nodes, MapValidationResult result)
    {
        if (nodes.Count == 0)
        {
            result.Errors.Add(new ValidationIssue
            {
                Rule = "无节点",
                Message = "地图没有任何节点"
            });
        }
    }

    private void ValidateNoEdges(List<MapEdgeListItemDto> edges, MapValidationResult result)
    {
        if (edges.Count == 0)
        {
            result.Errors.Add(new ValidationIssue
            {
                Rule = "无边",
                Message = "地图没有任何边"
            });
        }
    }

    private void ValidateIsolatedNodes(List<MapNodeListItemDto> nodes, List<MapEdgeListItemDto> edges, MapValidationResult result)
    {
        var connectedNodeIds = new HashSet<Guid>();

        foreach (var edge in edges)
        {
            connectedNodeIds.Add(edge.StartNodeId);
            connectedNodeIds.Add(edge.EndNodeId);
        }

        foreach (var node in nodes)
        {
            if (!connectedNodeIds.Contains(node.Id))
            {
                result.Errors.Add(new ValidationIssue
                {
                    Rule = "孤立节点",
                    Message = $"节点 {node.NodeCode} 没有任何边连接",
                    ElementId = node.Id,
                    ElementCode = node.NodeCode,
                    ElementType = "node"
                });
            }
        }
    }

    private void ValidateSelfLoopEdges(List<MapEdgeListItemDto> edges, MapValidationResult result)
    {
        foreach (var edge in edges)
        {
            if (edge.StartNodeId == edge.EndNodeId)
            {
                result.Errors.Add(new ValidationIssue
                {
                    Rule = "自环边",
                    Message = $"边 {edge.EdgeCode} 的起点与终点相同",
                    ElementId = edge.Id,
                    ElementCode = edge.EdgeCode,
                    ElementType = "edge"
                });
            }
        }
    }

    private void ValidateDuplicateEdges(List<MapEdgeListItemDto> edges, MapValidationResult result)
    {
        var edgeSet = new HashSet<(Guid, Guid)>();

        foreach (var edge in edges)
        {
            var key = (edge.StartNodeId, edge.EndNodeId);
            if (!edgeSet.Add(key))
            {
                result.Errors.Add(new ValidationIssue
                {
                    Rule = "重复边",
                    Message = $"边 {edge.EdgeCode} 与其他边重复（相同起点+终点）",
                    ElementId = edge.Id,
                    ElementCode = edge.EdgeCode,
                    ElementType = "edge"
                });
            }
        }
    }

    private void ValidateDeadEndNoExit(
        List<MapNodeListItemDto> nodes, 
        List<MapEdgeListItemDto> edges, 
        MapValidationResult result)
    {
        // 构建出边集合（考虑双向边）
        var nodesWithOutEdge = new HashSet<Guid>();
        var nodesWithInEdge = new HashSet<Guid>();

        foreach (var edge in edges)
        {
            nodesWithOutEdge.Add(edge.StartNodeId);
            nodesWithInEdge.Add(edge.EndNodeId);

            if (edge.IsBidirectional)
            {
                nodesWithOutEdge.Add(edge.EndNodeId);
                nodesWithInEdge.Add(edge.StartNodeId);
            }
        }

        foreach (var node in nodes)
        {
            // 有入边但没有出边
            if (nodesWithInEdge.Contains(node.Id) && !nodesWithOutEdge.Contains(node.Id))
            {
                result.Warnings.Add(new ValidationIssue
                {
                    Rule = "死胡同-无出口",
                    Message = $"节点 {node.NodeCode} 是死胡同（只有入边，没有出边）",
                    ElementId = node.Id,
                    ElementCode = node.NodeCode,
                    ElementType = "node"
                });
            }
        }
    }

    private void ValidateDeadEndNoEntry(
        List<MapNodeListItemDto> nodes, 
        List<MapEdgeListItemDto> edges, 
        MapValidationResult result)
    {
        var nodesWithOutEdge = new HashSet<Guid>();
        var nodesWithInEdge = new HashSet<Guid>();

        foreach (var edge in edges)
        {
            nodesWithOutEdge.Add(edge.StartNodeId);
            nodesWithInEdge.Add(edge.EndNodeId);

            if (edge.IsBidirectional)
            {
                nodesWithOutEdge.Add(edge.EndNodeId);
                nodesWithInEdge.Add(edge.StartNodeId);
            }
        }

        foreach (var node in nodes)
        {
            // 有出边但没有入边
            if (nodesWithOutEdge.Contains(node.Id) && !nodesWithInEdge.Contains(node.Id))
            {
                result.Warnings.Add(new ValidationIssue
                {
                    Rule = "死胡同-无入口",
                    Message = $"节点 {node.NodeCode} 无法到达（只有出边，没有入边）",
                    ElementId = node.Id,
                    ElementCode = node.NodeCode,
                    ElementType = "node"
                });
            }
        }
    }

    private void ValidateUnreachableNodes(
        List<MapNodeListItemDto> nodes, 
        List<MapEdgeListItemDto> edges, 
        MapValidationResult result)
    {
        if (nodes.Count == 0) return;

        // 构建邻接表（有向图）
        var adjacencyList = BuildAdjacencyList(nodes, edges);

        // 从第一个节点开始 BFS
        var startNode = nodes[0];
        var reachable = BfsReachable(startNode.Id, adjacencyList);

        foreach (var node in nodes)
        {
            if (!reachable.Contains(node.Id))
            {
                result.Warnings.Add(new ValidationIssue
                {
                    Rule = "不可达节点",
                    Message = $"节点 {node.NodeCode} 从 {startNode.NodeCode} 不可达",
                    ElementId = node.Id,
                    ElementCode = node.NodeCode,
                    ElementType = "node"
                });
            }
        }
    }

    private void ValidateNoReturnNodes(
        List<MapNodeListItemDto> nodes, 
        List<MapEdgeListItemDto> edges, 
        MapValidationResult result)
    {
        if (nodes.Count == 0) return;

        // 构建邻接表
        var adjacencyList = BuildAdjacencyList(nodes, edges);

        var startNode = nodes[0];
        var reachableFromStart = BfsReachable(startNode.Id, adjacencyList);

        // 检查每个可达节点是否能返回起点
        foreach (var node in nodes)
        {
            if (node.Id == startNode.Id) continue;
            if (!reachableFromStart.Contains(node.Id)) continue; // 不可达的节点已在上一步报告

            var reachableFromNode = BfsReachable(node.Id, adjacencyList);
            if (!reachableFromNode.Contains(startNode.Id))
            {
                result.Warnings.Add(new ValidationIssue
                {
                    Rule = "无法返回节点",
                    Message = $"从节点 {node.NodeCode} 无法返回 {startNode.NodeCode}",
                    ElementId = node.Id,
                    ElementCode = node.NodeCode,
                    ElementType = "node"
                });
            }
        }
    }

    /// <summary>
    /// 构建相邻列表
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="edges"></param>
    /// <returns></returns>
    private Dictionary<Guid, List<Guid>> BuildAdjacencyList(
        List<MapNodeListItemDto> nodes,
        List<MapEdgeListItemDto> edges)
    {
        var adjacencyList = new Dictionary<Guid, List<Guid>>();

        foreach (var node in nodes)
        {
            adjacencyList[node.Id] = [];
        }

        foreach (var edge in edges)
        {
            if (adjacencyList.ContainsKey(edge.StartNodeId))
            {
                adjacencyList[edge.StartNodeId].Add(edge.EndNodeId);
            }

            if (edge.IsBidirectional && adjacencyList.ContainsKey(edge.EndNodeId))
            {
                adjacencyList[edge.EndNodeId].Add(edge.StartNodeId);
            }
        }

        return adjacencyList;
    }

    /// <summary>
    /// 广度优先搜索（BFS）算法，用于查找从起始节点可达的所有节点
    /// </summary>
    /// <param name="startId"></param>
    /// <param name="adjacencyList"></param>
    /// <returns></returns>
    private HashSet<Guid> BfsReachable(Guid startId, Dictionary<Guid, List<Guid>> adjacencyList)
    {
        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();

        queue.Enqueue(startId);
        visited.Add(startId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (adjacencyList.TryGetValue(current, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return visited;
    }
}

/// <summary>
/// 验证结果
/// </summary>
public class MapValidationResult
{
    public List<ValidationIssue> Errors { get; set; } = [];
    public List<ValidationIssue> Warnings { get; set; } = [];
    public int NodeCount { get; set; }
    public int EdgeCount { get; set; }
    public string ConnectivityStatus { get; set; } = "未知";

    public bool IsValid => Errors.Count == 0;
    public bool HasWarnings => Warnings.Count > 0;
}

/// <summary>
/// 验证问题
/// </summary>
public class ValidationIssue
{
    public string Rule { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? ElementId { get; set; }
    public string? ElementCode { get; set; }
    public string? ElementType { get; set; }
}
