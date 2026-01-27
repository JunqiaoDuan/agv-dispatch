using AgvDispatch.Business.Entities.MapAggregate;
using AgvDispatch.Business.Entities.StationAggregate;
using AgvDispatch.Business.Entities.TaskRouteAggregate;
using AgvDispatch.Business.Specifications.MapEdges;
using AgvDispatch.Business.Specifications.MapNodes;
using AgvDispatch.Business.Specifications.Stations;
using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Extensions;
using AgvDispatch.Shared.PathFinding;
using AgvDispatch.Shared.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Business.Services;

/// <summary>
/// 任务路径规划服务实现
/// </summary>
public class TaskRouteService : ITaskRouteService
{
    private readonly IRepository<Station> _stationRepository;
    private readonly IRepository<MapNode> _nodeRepository;
    private readonly IRepository<MapEdge> _edgeRepository;
    private readonly IRepository<TaskRoute> _taskRouteRepository;
    private readonly IRepository<TaskRouteSegment> _taskRouteSegmentRepository;
    private readonly IRepository<TaskRouteCheckpoint> _taskRouteCheckpointRepository;
    private readonly ILogger<TaskRouteService> _logger;

    public TaskRouteService(
        IRepository<Station> stationRepository,
        IRepository<MapNode> nodeRepository,
        IRepository<MapEdge> edgeRepository,
        IRepository<TaskRoute> taskRouteRepository,
        IRepository<TaskRouteSegment> taskRouteSegmentRepository,
        IRepository<TaskRouteCheckpoint> taskRouteCheckpointRepository,
        ILogger<TaskRouteService> logger)
    {
        _stationRepository = stationRepository;
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
        _taskRouteRepository = taskRouteRepository;
        _taskRouteSegmentRepository = taskRouteSegmentRepository;
        _taskRouteCheckpointRepository = taskRouteCheckpointRepository;
        _logger = logger;
    }

    public async Task<TaskRoute?> CreateTaskRouteAsync(
        Guid taskId,
        string startStationCode,
        string endStationCode,
        Guid? userId)
    {
        // 1. 查找起始站点和终点站点
        var startStationSpec = new StationByStationCodeSpec(startStationCode);
        var startStation = await _stationRepository.FirstOrDefaultAsync(startStationSpec);
        if (startStation == null)
        {
            _logger.LogWarning("[TaskRouteService] 起始站点不存在: StationCode={StationCode}", startStationCode);
            return null;
        }

        var endStationSpec = new StationByStationCodeSpec(endStationCode);
        var endStation = await _stationRepository.FirstOrDefaultAsync(endStationSpec);
        if (endStation == null)
        {
            _logger.LogWarning("[TaskRouteService] 终点站点不存在: StationCode={StationCode}", endStationCode);
            return null;
        }

        // 2. 验证两个站点是否在同一地图
        if (startStation.MapId != endStation.MapId)
        {
            _logger.LogWarning("[TaskRouteService] 起始站点和终点站点不在同一地图: StartMapId={StartMapId}, EndMapId={EndMapId}",
                startStation.MapId, endStation.MapId);
            return null;
        }

        var mapId = startStation.MapId;

        // 3. 加载地图数据（节点、边、站点）
        var nodeSpec = new MapNodeListSpec(mapId);
        var nodes = await _nodeRepository.ListAsync(nodeSpec);
        var nodeDtos = nodes.Select(n => n.MapTo<MapNodeListItemDto>()).ToList();

        var edgeSpec = new MapEdgeListSpec(mapId);
        var edges = await _edgeRepository.ListAsync(edgeSpec);
        var edgeDtos = edges.Select(e =>
        {
            var dto = e.MapTo<MapEdgeListItemDto>();
            // 补充节点信息
            var startNode = nodes.FirstOrDefault(n => n.Id == e.StartNodeId);
            var endNode = nodes.FirstOrDefault(n => n.Id == e.EndNodeId);
            if (startNode != null)
            {
                dto.StartNodeCode = startNode.NodeCode;
                dto.StartNodeName = startNode.DisplayName;
            }
            if (endNode != null)
            {
                dto.EndNodeCode = endNode.NodeCode;
                dto.EndNodeName = endNode.DisplayName;
            }
            return dto;
        }).ToList();

        var stationSpec = new StationListSpec(mapId);
        var stations = await _stationRepository.ListAsync(stationSpec);
        var stationDtos = stations.Select(s => s.MapTo<StationListItemDto>()).ToList();

        // 4. 使用 A* 算法进行路径规划
        var pathfinder = new AStarPathfinder(nodeDtos, edgeDtos, stationDtos);
        var pathResult = pathfinder.FindPath(startStation.Id, endStation.Id);

        if (!pathResult.Success)
        {
            _logger.LogWarning("[TaskRouteService] 路径规划失败: {ErrorMessage}", pathResult.ErrorMessage);
            return null;
        }

        // 5. 创建 TaskRoute（聚合根）
        var taskRoute = new TaskRoute
        {
            Id = NewId.NextSequentialGuid(),
            TaskId = taskId,
            StartStationCode = startStationCode,
            EndStationCode = endStationCode,
            Description = $"{startStationCode} -> {endStationCode}",
            SortNo = 10
        };
        taskRoute.OnCreate(userId);
        await _taskRouteRepository.AddAsync(taskRoute);

        // 6. 创建 TaskRouteSegment（路径段，用于 Web 显示）
        var seq = 10;
        var _waitingAddSegments = new List<TaskRouteSegment>();
        foreach (var edgeId in pathResult.EdgePath)
        {
            var edge = edges.FirstOrDefault(e => e.Id == edgeId);
            if (edge == null)
                continue;

            // 判断行驶方向
            var direction = DriveDirection.Forward;
            if (seq > 10)
            {
                var prevEdgeId = pathResult.EdgePath[pathResult.EdgePath.IndexOf(edgeId) - 1];
                var prevEdge = edges.FirstOrDefault(e => e.Id == prevEdgeId);
                if (prevEdge != null && prevEdge.EndNodeId == edge.EndNodeId)
                {
                    direction = DriveDirection.Backward;
                }
            }

            // 判断是否是最后一段（需要停车）
            var isLastSegment = edgeId == pathResult.EdgePath.Last();
            var finalAction = isLastSegment ? FinalAction.Stop : FinalAction.None;

            var segment = new TaskRouteSegment
            {
                Id = NewId.NextSequentialGuid(),
                TaskRouteId = taskRoute.Id,
                Seq = seq,
                MapEdgeId = edgeId,
                Direction = direction,
                FinalAction = finalAction
            };
            segment.OnCreate(userId);
            _waitingAddSegments.Add(segment);

            seq += 10;
        }
        if (_waitingAddSegments.Any())
        {
            await _taskRouteSegmentRepository.AddRangeAsync(_waitingAddSegments);
        }

        // 7. 创建 TaskRouteCheckpoint（检查点，用于 AGV 导航）
        var checkpointSeq = 10;
        var checkpointStations = new List<string> { startStationCode };

        // 添加中间站点（如果路径经过其他站点）
        foreach (var nodeId in pathResult.NodePath.Skip(1).SkipLast(1))
        {
            var station = stations.FirstOrDefault(s => s.NodeId == nodeId);
            if (station != null)
            {
                checkpointStations.Add(station.StationCode);
            }
        }

        // 添加终点站点
        checkpointStations.Add(endStationCode);

        // 创建检查点
        var _waitingAddCheckpoints = new List<TaskRouteCheckpoint>();
        for (int i = 0; i < checkpointStations.Count; i++)
        {
            var stationCode = checkpointStations[i];
            var checkpointType = i == 0 ? CheckpointType.Start :
                                 i == checkpointStations.Count - 1 ? CheckpointType.End :
                                 CheckpointType.Middle;

            var checkpoint = new TaskRouteCheckpoint
            {
                Id = NewId.NextSequentialGuid(),
                TaskRouteId = taskRoute.Id,
                Seq = checkpointSeq,
                StationCode = stationCode,
                CheckpointType = checkpointType
            };
            checkpoint.OnCreate(userId);
            _waitingAddCheckpoints.Add(checkpoint);

            checkpointSeq += 10;
        }
        if (_waitingAddCheckpoints.Any())
        {
            await _taskRouteCheckpointRepository.AddRangeAsync(_waitingAddCheckpoints);
        }

        _logger.LogInformation("[TaskRouteService] 任务路径创建成功: TaskId={TaskId}, RouteId={RouteId}, Segments={SegmentCount}, Checkpoints={CheckpointCount}",
            taskId, taskRoute.Id, pathResult.EdgePath.Count, checkpointStations.Count);

        return taskRoute;
    }
}
