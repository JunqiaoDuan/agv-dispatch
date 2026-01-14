using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.PathFinding;

namespace AgvDispatch.Shared.Simulation;

/// <summary>
/// AGV 模拟器（核心引擎）
/// </summary>
public class AgvSimulator
{
    private readonly PathfindingResult _path;
    private readonly List<MapNodeListItemDto> _nodes;
    private readonly List<MapEdgeListItemDto> _edges;
    private readonly AgvSimulationConfig _config;
    private readonly Dictionary<Guid, MapNodeListItemDto> _nodeDict;
    private readonly Dictionary<Guid, MapEdgeListItemDto> _edgeDict;

    private SimulationState _state = SimulationState.NotStarted;
    private AgvPosition _currentPosition;
    private DateTime _lastUpdateTime;
    private decimal _accumulatedDistance;

    /// <summary>
    /// 当前状态
    /// </summary>
    public SimulationState State => _state;

    /// <summary>
    /// 当前位置
    /// </summary>
    public AgvPosition CurrentPosition => _currentPosition;

    /// <summary>
    /// 路径信息
    /// </summary>
    public PathfindingResult Path => _path;

    /// <summary>
    /// 位置更新事件
    /// </summary>
    public event Action<AgvPosition>? OnPositionUpdated;

    /// <summary>
    /// 完成事件
    /// </summary>
    public event Action? OnCompleted;

    /// <summary>
    /// 失败事件
    /// </summary>
    public event Action<string>? OnFailed;

    public AgvSimulator(
        PathfindingResult path,
        List<MapNodeListItemDto> nodes,
        List<MapEdgeListItemDto> edges,
        AgvSimulationConfig config)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        _edges = edges ?? throw new ArgumentNullException(nameof(edges));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        if (!path.Success)
            throw new ArgumentException("路径查找失败，无法创建模拟器", nameof(path));

        _nodeDict = nodes.ToDictionary(n => n.Id);
        _edgeDict = edges.ToDictionary(e => e.Id);

        // 初始化位置为起点
        var startNodeId = path.NodePath.First();
        var startNode = _nodeDict[startNodeId];
        _currentPosition = new AgvPosition
        {
            X = startNode.X,
            Y = startNode.Y,
            Angle = 0,
            CurrentEdgeIndex = 0,
            EdgeProgress = 0,
            OverallProgress = 0,
            TraveledDistance = 0
        };
    }

    /// <summary>
    /// 启动模拟
    /// </summary>
    public void Start()
    {
        if (_state != SimulationState.NotStarted && _state != SimulationState.Paused)
            throw new InvalidOperationException($"无法启动：当前状态为 {_state}");

        _state = SimulationState.Running;
        _lastUpdateTime = DateTime.Now;
    }

    /// <summary>
    /// 暂停模拟
    /// </summary>
    public void Pause()
    {
        if (_state != SimulationState.Running)
            throw new InvalidOperationException($"无法暂停：当前状态为 {_state}");

        _state = SimulationState.Paused;
    }

    /// <summary>
    /// 恢复模拟
    /// </summary>
    public void Resume()
    {
        if (_state != SimulationState.Paused)
            throw new InvalidOperationException($"无法恢复：当前状态为 {_state}");

        _state = SimulationState.Running;
        _lastUpdateTime = DateTime.Now;
    }

    /// <summary>
    /// 停止模拟
    /// </summary>
    public void Stop()
    {
        _state = SimulationState.NotStarted;
        _accumulatedDistance = 0;

        // 重置到起点
        var startNodeId = _path.NodePath.First();
        var startNode = _nodeDict[startNodeId];
        _currentPosition = new AgvPosition
        {
            X = startNode.X,
            Y = startNode.Y,
            Angle = 0,
            CurrentEdgeIndex = 0,
            EdgeProgress = 0,
            OverallProgress = 0,
            TraveledDistance = 0
        };
    }

    /// <summary>
    /// 更新速度
    /// </summary>
    /// <param name="newSpeed">新的速度（厘米/秒）</param>
    public void UpdateSpeed(decimal newSpeed)
    {
        if (newSpeed <= 0)
            throw new ArgumentException("速度必须大于0", nameof(newSpeed));

        _config.Speed = newSpeed;
    }

    /// <summary>
    /// 更新模拟（定时调用）
    /// </summary>
    public void Update()
    {
        if (_state != SimulationState.Running)
            return;

        var now = DateTime.Now;
        var deltaTime = (decimal)(now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        // 计算距离增量
        var distanceIncrement = _config.Speed * deltaTime;
        _accumulatedDistance += distanceIncrement;
        _currentPosition.TraveledDistance = _accumulatedDistance;

        // 计算总进度
        var overallProgress = _accumulatedDistance / _path.TotalDistance;
        if (overallProgress >= 1m)
        {
            // 到达终点
            var finalNodeId = _path.NodePath.Last();
            var finalNode = _nodeDict[finalNodeId];
            _currentPosition.X = finalNode.X;
            _currentPosition.Y = finalNode.Y;
            _currentPosition.OverallProgress = 1m;
            _currentPosition.CurrentEdgeIndex = _path.EdgePath.Count - 1;
            _currentPosition.EdgeProgress = 1m;

            _state = SimulationState.Completed;
            OnPositionUpdated?.Invoke(_currentPosition);
            OnCompleted?.Invoke();
            return;
        }

        _currentPosition.OverallProgress = overallProgress;

        // 映射到当前边索引和边内进度
        var (edgeIndex, edgeProgress) = MapProgressToEdge(_accumulatedDistance);
        _currentPosition.CurrentEdgeIndex = edgeIndex;
        _currentPosition.EdgeProgress = edgeProgress;

        // 插值计算位置和角度
        var edgeId = _path.EdgePath[edgeIndex];
        var edge = _edgeDict[edgeId];
        var startNodeId = _path.NodePath[edgeIndex];
        var endNodeId = _path.NodePath[edgeIndex + 1];
        var startNode = _nodeDict[startNodeId];
        var endNode = _nodeDict[endNodeId];

        var (x, y, angle) = PathInterpolator.Interpolate(edge, startNode, endNode, edgeProgress);
        _currentPosition.X = x;
        _currentPosition.Y = y;
        _currentPosition.Angle = angle;

        OnPositionUpdated?.Invoke(_currentPosition);
    }

    /// <summary>
    /// 将总距离映射到边索引和边内进度
    /// </summary>
    private (int edgeIndex, decimal edgeProgress) MapProgressToEdge(decimal traveledDistance)
    {
        decimal accumulatedEdgeDistance = 0;

        for (int i = 0; i < _path.EdgePath.Count; i++)
        {
            var edgeId = _path.EdgePath[i];
            var edge = _edgeDict[edgeId];
            var edgeLength = edge.Distance;

            if (traveledDistance <= accumulatedEdgeDistance + edgeLength)
            {
                var edgeProgress = edgeLength > 0
                    ? (traveledDistance - accumulatedEdgeDistance) / edgeLength
                    : 1m;
                return (i, Math.Clamp(edgeProgress, 0m, 1m));
            }

            accumulatedEdgeDistance += edgeLength;
        }

        // 已超出最后一条边
        return (_path.EdgePath.Count - 1, 1m);
    }
}
