using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.DTOs.Maps;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.Rendering;

/// <summary>
/// 地图验证器 - 验证地图配置是否符合 AGV 调度要求
/// </summary>
public class MapValidator
{
    /// <summary>
    /// 验证地图
    /// </summary>
    public MapValidationResult Validate(
        List<MapNodeListItemDto> nodes,
        List<MapEdgeListItemDto> edges,
        MapDetailDto? map = null,
        List<StationListItemDto>? stations = null)
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

        // 新增：几何/空间验证
        if (map != null)
        {
            ValidateNodesOutOfBounds(nodes, map, result);
        }

        // 新增：站点关联验证
        if (stations != null)
        {
            ValidateStationNodeAssociation(nodes, stations, result);
        }

        // 警告级别验证
        ValidateDeadEndNoExit(nodes, edges, result);
        ValidateDeadEndNoEntry(nodes, edges, result);
        ValidateUnreachableNodes(nodes, edges, result);
        ValidateNoReturnNodes(nodes, edges, result);

        // 新增：几何/空间警告
        ValidateOverlappingNodes(nodes, result);
        ValidateEdgeDistanceAccuracy(nodes, edges, result);

        // 新增：连通性增强验证
        ValidateConnectedComponents(nodes, edges, result);

        // 新增：关键节点连通性
        if (stations != null)
        {
            ValidateCriticalNodeConnectivity(nodes, edges, stations, result);
        }

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

    // ========== 新增验证方法 ==========

    /// <summary>
    /// 验证节点是否超出地图边界
    /// </summary>
    private void ValidateNodesOutOfBounds(
        List<MapNodeListItemDto> nodes,
        MapDetailDto map,
        MapValidationResult result)
    {
        foreach (var node in nodes)
        {
            if (node.X < 0 || node.X > map.Width || node.Y < 0 || node.Y > map.Height)
            {
                result.Errors.Add(new ValidationIssue
                {
                    Rule = "节点超出边界",
                    Message = $"节点 {node.NodeCode} 坐标 ({node.X}, {node.Y}) 超出地图边界 ({map.Width} x {map.Height})",
                    ElementId = node.Id,
                    ElementCode = node.NodeCode,
                    ElementType = "node"
                });
            }
        }
    }

    /// <summary>
    /// 验证节点位置重叠（距离过近）
    /// </summary>
    private void ValidateOverlappingNodes(
        List<MapNodeListItemDto> nodes,
        MapValidationResult result)
    {
        const decimal minDistance = 0.1m; // 最小节点间距（厘米）

        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                var node1 = nodes[i];
                var node2 = nodes[j];

                var distance = CalculateDistance(node1.X, node1.Y, node2.X, node2.Y);

                if (distance < minDistance)
                {
                    result.Warnings.Add(new ValidationIssue
                    {
                        Rule = "节点位置重叠",
                        Message = $"节点 {node1.NodeCode} 和 {node2.NodeCode} 位置过近 (距离: {distance:F2}cm < {minDistance}cm)",
                        ElementId = node1.Id,
                        ElementCode = node1.NodeCode,
                        ElementType = "node"
                    });
                }
            }
        }
    }

    /// <summary>
    /// 验证边距离计算是否准确
    /// </summary>
    private void ValidateEdgeDistanceAccuracy(
        List<MapNodeListItemDto> nodes,
        List<MapEdgeListItemDto> edges,
        MapValidationResult result)
    {
        const decimal tolerance = 0.01m; // 允许的误差百分比（1%）

        var nodeDict = nodes.ToDictionary(n => n.Id);

        foreach (var edge in edges)
        {
            if (!nodeDict.TryGetValue(edge.StartNodeId, out var startNode) ||
                !nodeDict.TryGetValue(edge.EndNodeId, out var endNode))
            {
                continue; // 节点不存在的情况已在其他验证中处理
            }

            var straightDistance = CalculateDistance(startNode.X, startNode.Y, endNode.X, endNode.Y);

            // 对于曲线边（有弧度），验证路径长度的合理范围
            if (edge.Curvature.HasValue && edge.Curvature.Value != 0)
            {
                // 曲线边的长度应该：
                // - 最小值：直线距离（不能比直线更短）
                // - 最大值：2倍直线距离（半圆弧长约为1.57倍，2倍是安全上限）
                var minAllowedDistance = straightDistance;
                var maxAllowedDistance = straightDistance * 2;

                if (edge.Distance < minAllowedDistance)
                {
                    result.Warnings.Add(new ValidationIssue
                    {
                        Rule = "曲线边距离不合理",
                        Message = $"曲线边 {edge.EdgeCode} 存储距离 ({edge.Distance:F2}cm) 小于直线距离 ({straightDistance:F2}cm)，不符合曲线特性",
                        ElementId = edge.Id,
                        ElementCode = edge.EdgeCode,
                        ElementType = "edge"
                    });
                }
                else if (edge.Distance > maxAllowedDistance)
                {
                    result.Warnings.Add(new ValidationIssue
                    {
                        Rule = "曲线边距离不合理",
                        Message = $"曲线边 {edge.EdgeCode} 存储距离 ({edge.Distance:F2}cm) 超过直线距离的2倍 ({maxAllowedDistance:F2}cm)，曲率可能过大",
                        ElementId = edge.Id,
                        ElementCode = edge.EdgeCode,
                        ElementType = "edge"
                    });
                }
                continue; // 曲线边不做精确匹配验证
            }

            // 直线边：精确验证存储距离与实际距离
            var difference = Math.Abs(straightDistance - edge.Distance);
            var percentDifference = straightDistance > 0 ? difference / straightDistance : 0;

            if (percentDifference > tolerance)
            {
                result.Warnings.Add(new ValidationIssue
                {
                    Rule = "边距离不准确",
                    Message = $"边 {edge.EdgeCode} 存储距离 ({edge.Distance:F2}cm) 与实际计算距离 ({straightDistance:F2}cm) 差异过大 ({percentDifference:P1})",
                    ElementId = edge.Id,
                    ElementCode = edge.EdgeCode,
                    ElementType = "edge"
                });
            }
        }
    }

    /// <summary>
    /// 验证站点关联的节点是否有效
    /// </summary>
    private void ValidateStationNodeAssociation(
        List<MapNodeListItemDto> nodes,
        List<StationListItemDto> stations,
        MapValidationResult result)
    {
        var nodeIds = nodes.Select(n => n.Id).ToHashSet();

        foreach (var station in stations)
        {
            if (!nodeIds.Contains(station.NodeId))
            {
                result.Errors.Add(new ValidationIssue
                {
                    Rule = "站点关联节点无效",
                    Message = $"站点 {station.StationCode} 关联的节点 (ID: {station.NodeId}) 不存在于当前地图",
                    ElementId = station.Id,
                    ElementCode = station.StationCode,
                    ElementType = "station"
                });
            }
        }
    }

    /// <summary>
    /// 验证关键节点（如充电站）的连通性
    /// </summary>
    private void ValidateCriticalNodeConnectivity(
        List<MapNodeListItemDto> nodes,
        List<MapEdgeListItemDto> edges,
        List<StationListItemDto> stations,
        MapValidationResult result)
    {
        // 识别关键站点类型
        var criticalStationTypes = new[]
        {
            StationType.Charge, // 充电站
            StationType.Standby // 待命点
        };

        var criticalStations = stations
            .Where(s => criticalStationTypes.Contains(s.StationType))
            .ToList();

        if (criticalStations.Count == 0)
        {
            return; // 没有关键站点，跳过验证
        }

        // 构建邻接表
        var adjacencyList = BuildAdjacencyList(nodes, edges);

        // 检查每个关键站点是否可以相互到达
        foreach (var station in criticalStations)
        {
            var reachable = BfsReachable(station.NodeId, adjacencyList);

            // 检查是否可以到达其他关键站点
            var unreachableStations = criticalStations
                .Where(s => s.Id != station.Id && !reachable.Contains(s.NodeId))
                .ToList();

            if (unreachableStations.Any())
            {
                var unreachableNames = string.Join(", ", unreachableStations.Select(s => s.StationCode));
                result.Errors.Add(new ValidationIssue
                {
                    Rule = "关键站点不连通",
                    Message = $"关键站点 {station.StationCode} ({station.StationType.ToDisplayText()}) 无法到达: {unreachableNames}",
                    ElementId = station.Id,
                    ElementCode = station.StationCode,
                    ElementType = "station"
                });
            }
        }
    }

    /// <summary>
    /// 检测多连通分量（互不连通的子图）
    /// </summary>
    private void ValidateConnectedComponents(
        List<MapNodeListItemDto> nodes,
        List<MapEdgeListItemDto> edges,
        MapValidationResult result)
    {
        if (nodes.Count == 0) return;

        var adjacencyList = BuildAdjacencyList(nodes, edges);
        var visited = new HashSet<Guid>();
        var components = new List<List<Guid>>();

        // 使用 DFS 找出所有连通分量
        foreach (var node in nodes)
        {
            if (!visited.Contains(node.Id))
            {
                var component = new List<Guid>();
                DfsVisit(node.Id, adjacencyList, visited, component);
                components.Add(component);
            }
        }

        // 如果有多个连通分量，报告警告
        if (components.Count > 1)
        {
            var componentDetails = components
                .Select((comp, index) =>
                {
                    var nodeNames = comp.Take(3)
                        .Select(id => nodes.First(n => n.Id == id).NodeCode)
                        .ToList();
                    var nodeDisplay = string.Join(", ", nodeNames);
                    if (comp.Count > 3)
                    {
                        nodeDisplay += $" 等 {comp.Count} 个节点";
                    }
                    return $"子图{index + 1}: {nodeDisplay}";
                })
                .ToList();

            result.Warnings.Add(new ValidationIssue
            {
                Rule = "多连通分量",
                Message = $"地图包含 {components.Count} 个互不连通的子图。{string.Join("; ", componentDetails)}",
            });
        }
    }

    /// <summary>
    /// 深度优先遍历
    /// </summary>
    private void DfsVisit(
        Guid nodeId,
        Dictionary<Guid, List<Guid>> adjacencyList,
        HashSet<Guid> visited,
        List<Guid> component)
    {
        visited.Add(nodeId);
        component.Add(nodeId);

        if (adjacencyList.TryGetValue(nodeId, out var neighbors))
        {
            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    DfsVisit(neighbor, adjacencyList, visited, component);
                }
            }
        }
    }

    /// <summary>
    /// 计算两点之间的欧几里得距离
    /// </summary>
    private decimal CalculateDistance(decimal x1, decimal y1, decimal x2, decimal y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return (decimal)Math.Sqrt((double)(dx * dx + dy * dy));
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
