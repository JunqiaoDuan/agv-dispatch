using AgvDispatch.Business.Entities.MapAggregate;
using AgvDispatch.Business.Entities.StationAggregate;
using AgvDispatch.Business.Specifications.MapEdges;
using AgvDispatch.Business.Specifications.MapNodes;
using AgvDispatch.Business.Specifications.Maps;
using AgvDispatch.Business.Specifications.Stations;
using AgvDispatch.Shared.Constants;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.DTOs.Maps;
using AgvDispatch.Shared.Extensions;
using AgvDispatch.Shared.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgvDispatch.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MapsController : ControllerBase
{
    private readonly IRepository<Map> _mapRepository;
    private readonly IRepository<MapNode> _nodeRepository;
    private readonly IRepository<MapEdge> _edgeRepository;
    private readonly IRepository<Station> _stationRepository;
    private readonly ILogger<MapsController> _logger;

    public MapsController(
        IRepository<Map> mapRepository,
        IRepository<MapNode> nodeRepository,
        IRepository<MapEdge> edgeRepository,
        IRepository<Station> stationRepository,
        ILogger<MapsController> logger)
    {
        _mapRepository = mapRepository;
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
        _stationRepository = stationRepository;
        _logger = logger;
    }

    #region 地图 CRUD

    /// <summary>
    /// 获取所有地图列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MapListItemDto>>>> GetAll()
    {
        var spec = new MapListSpec();
        var maps = await _mapRepository.ListAsync(spec);

        var items = new List<MapListItemDto>();
        foreach (var map in maps)
        {
            var nodeCount = await _nodeRepository.CountAsync(new MapNodeCountSpec(map.Id));
            var edgeCount = await _edgeRepository.CountAsync(new MapEdgeCountSpec(map.Id));
            var stationCount = await _stationRepository.CountAsync(new StationCountSpec(map.Id));

            var dto = map.MapTo<MapListItemDto>();
            dto.NodeCount = nodeCount;
            dto.EdgeCount = edgeCount;
            dto.RouteCount = 0; // 旧Route已删除，暂时设为0
            dto.StationCount = stationCount;
            items.Add(dto);
        }

        return Ok(ApiResponse<List<MapListItemDto>>.Ok(items));
    }

    /// <summary>
    /// 获取地图详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MapDetailDto>>> GetById(Guid id)
    {
        var spec = new MapByIdSpec(id);
        var map = await _mapRepository.FirstOrDefaultAsync(spec);

        if (map == null)
        {
            return NotFound(ApiResponse<MapDetailDto>.Fail("地图不存在"));
        }

        var nodeCount = await _nodeRepository.CountAsync(new MapNodeCountSpec(map.Id));
        var edgeCount = await _edgeRepository.CountAsync(new MapEdgeCountSpec(map.Id));

        var dto = map.MapTo<MapDetailDto>();
        dto.NodeCount = nodeCount;
        dto.EdgeCount = edgeCount;

        return Ok(ApiResponse<MapDetailDto>.Ok(dto));
    }

    /// <summary>
    /// 创建地图
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<MapDetailDto>>> Create([FromBody] CreateMapRequest request)
    {
        var codeSpec = new MapCodeExistsSpec(request.MapCode);
        var exists = await _mapRepository.AnyAsync(codeSpec);

        if (exists)
        {
            return BadRequest(ApiResponse<MapDetailDto>.Fail($"地图编号 {request.MapCode} 已存在"));
        }

        var map = new Map
        {
            MapCode = request.MapCode,
            DisplayName = request.DisplayName,
            Description = request.Description,
            Width = request.Width,
            Height = request.Height,
            SortNo = request.SortNo,
            IsActive = true
        };

        map.OnCreate();

        await _mapRepository.AddAsync(map);
        await _mapRepository.SaveChangesAsync();

        _logger.LogInformation("创建地图成功: {MapCode}", map.MapCode);

        var dto = map.MapTo<MapDetailDto>();
        return CreatedAtAction(nameof(GetById), new { id = map.Id }, ApiResponse<MapDetailDto>.Ok(dto, "创建成功"));
    }

    /// <summary>
    /// 更新地图
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<MapDetailDto>>> Update(Guid id, [FromBody] UpdateMapRequest request)
    {
        var spec = new MapByIdSpec(id);
        var map = await _mapRepository.FirstOrDefaultAsync(spec);

        if (map == null)
        {
            return NotFound(ApiResponse<MapDetailDto>.Fail("地图不存在"));
        }

        map.DisplayName = request.DisplayName;
        map.Description = request.Description;
        map.Width = request.Width;
        map.Height = request.Height;
        map.IsActive = request.IsActive;
        map.SortNo = request.SortNo;

        map.OnUpdate();

        await _mapRepository.UpdateAsync(map);
        await _mapRepository.SaveChangesAsync();

        _logger.LogInformation("更新地图成功: {MapCode}", map.MapCode);

        var nodeCount = await _nodeRepository.CountAsync(new MapNodeCountSpec(map.Id));
        var edgeCount = await _edgeRepository.CountAsync(new MapEdgeCountSpec(map.Id));

        var dto = map.MapTo<MapDetailDto>();
        dto.NodeCount = nodeCount;
        dto.EdgeCount = edgeCount;

        return Ok(ApiResponse<MapDetailDto>.Ok(dto, "更新成功"));
    }

    /// <summary>
    /// 删除地图（软删除）
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var spec = new MapByIdSpec(id);
        var map = await _mapRepository.FirstOrDefaultAsync(spec);

        if (map == null)
        {
            return NotFound(ApiResponse<bool>.Fail("地图不存在"));
        }

        // 检查是否有关联的路线
        // var routeSpec = new RouteByMapIdSpec(id); // 旧Route已删除
        // var hasRoutes = await _routeRepository.AnyAsync(routeSpec);
        // if (hasRoutes)
        // {
        //     return BadRequest(ApiResponse<bool>.Fail("该地图存在关联的路线，请先删除相关路线"));
        // }

        map.OnDelete("用户删除");

        await _mapRepository.UpdateAsync(map);
        await _mapRepository.SaveChangesAsync();

        _logger.LogInformation("删除地图成功: {MapCode}", map.MapCode);

        return Ok(ApiResponse<bool>.Ok(true, "删除成功"));
    }

    /// <summary>
    /// 获取下一个可用的地图编号
    /// </summary>
    [HttpGet("next-code")]
    public async Task<ActionResult<ApiResponse<string>>> GetNextCode()
    {
        var spec = new MapMaxCodeSpec();
        var map = await _mapRepository.FirstOrDefaultAsync(spec);

        int nextSeq = 1;
        if (map != null)
        {
            var seq = EntityCodes.ParseSequence(map.MapCode, EntityCodes.MapPrefix);
            if (seq.HasValue)
            {
                nextSeq = seq.Value + 1;
            }
        }

        var nextCode = EntityCodes.Generate(EntityCodes.MapPrefix, nextSeq);
        return Ok(ApiResponse<string>.Ok(nextCode));
    }

    #endregion

    #region 节点 CRUD

    /// <summary>
    /// 获取地图的所有节点
    /// </summary>
    [HttpGet("{mapId:guid}/nodes")]
    public async Task<ActionResult<ApiResponse<List<MapNodeListItemDto>>>> GetNodes(Guid mapId)
    {
        var mapSpec = new MapByIdSpec(mapId);
        var mapExists = await _mapRepository.AnyAsync(mapSpec);
        if (!mapExists)
        {
            return NotFound(ApiResponse<List<MapNodeListItemDto>>.Fail("地图不存在"));
        }

        var spec = new MapNodeListSpec(mapId);
        var nodes = await _nodeRepository.ListAsync(spec);
        var items = nodes.Select(x => x.MapTo<MapNodeListItemDto>()).ToList();

        return Ok(ApiResponse<List<MapNodeListItemDto>>.Ok(items));
    }

    /// <summary>
    /// 创建节点
    /// </summary>
    [HttpPost("{mapId:guid}/nodes")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<MapNodeListItemDto>>> CreateNode(Guid mapId, [FromBody] CreateMapNodeRequest request)
    {
        var mapSpec = new MapByIdSpec(mapId);
        var mapExists = await _mapRepository.AnyAsync(mapSpec);
        if (!mapExists)
        {
            return NotFound(ApiResponse<MapNodeListItemDto>.Fail("地图不存在"));
        }

        var codeSpec = new MapNodeCodeExistsSpec(mapId, request.NodeCode);
        var exists = await _nodeRepository.AnyAsync(codeSpec);
        if (exists)
        {
            return BadRequest(ApiResponse<MapNodeListItemDto>.Fail($"节点编号 {request.NodeCode} 已存在"));
        }

        var node = new MapNode
        {
            MapId = mapId,
            NodeCode = request.NodeCode,
            DisplayName = request.DisplayName,
            X = request.X,
            Y = request.Y,
            Remark = request.Remark,
            SortNo = request.SortNo
        };

        node.OnCreate();

        await _nodeRepository.AddAsync(node);
        await _nodeRepository.SaveChangesAsync();

        _logger.LogInformation("创建节点成功: {NodeCode} in Map {MapId}", node.NodeCode, mapId);

        var dto = node.MapTo<MapNodeListItemDto>();
        return Ok(ApiResponse<MapNodeListItemDto>.Ok(dto, "创建成功"));
    }

    /// <summary>
    /// 更新节点
    /// </summary>
    [HttpPut("{mapId:guid}/nodes/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<MapNodeListItemDto>>> UpdateNode(Guid mapId, Guid id, [FromBody] UpdateMapNodeRequest request)
    {
        var spec = new MapNodeByIdSpec(id);
        var node = await _nodeRepository.FirstOrDefaultAsync(spec);

        if (node == null || node.MapId != mapId)
        {
            return NotFound(ApiResponse<MapNodeListItemDto>.Fail("节点不存在"));
        }

        node.DisplayName = request.DisplayName;
        node.X = request.X;
        node.Y = request.Y;
        node.Remark = request.Remark;
        node.SortNo = request.SortNo;

        node.OnUpdate();

        await _nodeRepository.UpdateAsync(node);
        await _nodeRepository.SaveChangesAsync();

        _logger.LogInformation("更新节点成功: {NodeCode}", node.NodeCode);

        var dto = node.MapTo<MapNodeListItemDto>();
        return Ok(ApiResponse<MapNodeListItemDto>.Ok(dto, "更新成功"));
    }

    /// <summary>
    /// 删除节点
    /// </summary>
    [HttpDelete("{mapId:guid}/nodes/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteNode(Guid mapId, Guid id)
    {
        var spec = new MapNodeByIdSpec(id);
        var node = await _nodeRepository.FirstOrDefaultAsync(spec);

        if (node == null || node.MapId != mapId)
        {
            return NotFound(ApiResponse<bool>.Fail("节点不存在"));
        }

        // 检查是否有关联的边
        var edgeSpec = new MapEdgeByNodeIdSpec(id);
        var hasEdges = await _edgeRepository.AnyAsync(edgeSpec);
        if (hasEdges)
        {
            return BadRequest(ApiResponse<bool>.Fail("该节点存在关联的边，请先删除相关边"));
        }

        // 检查是否有关联的站点
        var stationSpec = new StationByNodeIdSpec(id);
        var hasStations = await _stationRepository.AnyAsync(stationSpec);
        if (hasStations)
        {
            return BadRequest(ApiResponse<bool>.Fail("该节点存在关联的站点，请先删除相关站点"));
        }

        node.OnDelete("用户删除");

        await _nodeRepository.UpdateAsync(node);
        await _nodeRepository.SaveChangesAsync();

        _logger.LogInformation("删除节点成功: {NodeCode}", node.NodeCode);

        return Ok(ApiResponse<bool>.Ok(true, "删除成功"));
    }

    /// <summary>
    /// 获取下一个可用的节点编号
    /// </summary>
    [HttpGet("{mapId:guid}/nodes/next-code")]
    public async Task<ActionResult<ApiResponse<string>>> GetNextNodeCode(Guid mapId)
    {
        var spec = new MapNodeMaxCodeSpec(mapId);
        var node = await _nodeRepository.FirstOrDefaultAsync(spec);

        int nextSeq = 1;
        if (node != null)
        {
            var seq = EntityCodes.ParseSequence(node.NodeCode, EntityCodes.NodePrefix);
            if (seq.HasValue)
            {
                nextSeq = seq.Value + 1;
            }
        }

        var nextCode = EntityCodes.Generate(EntityCodes.NodePrefix, nextSeq);
        return Ok(ApiResponse<string>.Ok(nextCode));
    }

    #endregion

    #region 边 CRUD

    /// <summary>
    /// 获取地图的所有边
    /// </summary>
    [HttpGet("{mapId:guid}/edges")]
    public async Task<ActionResult<ApiResponse<List<MapEdgeListItemDto>>>> GetEdges(Guid mapId)
    {
        var mapSpec = new MapByIdSpec(mapId);
        var mapExists = await _mapRepository.AnyAsync(mapSpec);
        if (!mapExists)
        {
            return NotFound(ApiResponse<List<MapEdgeListItemDto>>.Fail("地图不存在"));
        }

        var edgeSpec = new MapEdgeListSpec(mapId);
        var edges = await _edgeRepository.ListAsync(edgeSpec);

        var nodeSpec = new MapNodeListSpec(mapId);
        var nodes = await _nodeRepository.ListAsync(nodeSpec);
        var nodeDict = nodes.ToDictionary(x => x.Id, x => x);

        var items = edges.Select(edge =>
        {
            var dto = edge.MapTo<MapEdgeListItemDto>();
            if (nodeDict.TryGetValue(edge.StartNodeId, out var startNode))
            {
                dto.StartNodeCode = startNode.NodeCode;
                dto.StartNodeName = startNode.DisplayName;
            }
            if (nodeDict.TryGetValue(edge.EndNodeId, out var endNode))
            {
                dto.EndNodeCode = endNode.NodeCode;
                dto.EndNodeName = endNode.DisplayName;
            }
            return dto;
        }).ToList();

        return Ok(ApiResponse<List<MapEdgeListItemDto>>.Ok(items));
    }

    /// <summary>
    /// 创建边
    /// </summary>
    [HttpPost("{mapId:guid}/edges")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<MapEdgeListItemDto>>> CreateEdge(Guid mapId, [FromBody] CreateMapEdgeRequest request)
    {
        var mapSpec = new MapByIdSpec(mapId);
        var mapExists = await _mapRepository.AnyAsync(mapSpec);
        if (!mapExists)
        {
            return NotFound(ApiResponse<MapEdgeListItemDto>.Fail("地图不存在"));
        }

        var codeSpec = new MapEdgeCodeExistsSpec(mapId, request.EdgeCode);
        var exists = await _edgeRepository.AnyAsync(codeSpec);
        if (exists)
        {
            return BadRequest(ApiResponse<MapEdgeListItemDto>.Fail($"边编号 {request.EdgeCode} 已存在"));
        }

        // 验证起点和终点节点存在
        var startNodeSpec = new MapNodeByIdSpec(request.StartNodeId);
        var startNode = await _nodeRepository.FirstOrDefaultAsync(startNodeSpec);
        if (startNode == null || startNode.MapId != mapId)
        {
            return BadRequest(ApiResponse<MapEdgeListItemDto>.Fail("起点节点不存在"));
        }

        var endNodeSpec = new MapNodeByIdSpec(request.EndNodeId);
        var endNode = await _nodeRepository.FirstOrDefaultAsync(endNodeSpec);
        if (endNode == null || endNode.MapId != mapId)
        {
            return BadRequest(ApiResponse<MapEdgeListItemDto>.Fail("终点节点不存在"));
        }

        // 计算距离
        var distance = CalculateDistance(startNode, endNode, request);

        var edge = new MapEdge
        {
            MapId = mapId,
            EdgeCode = request.EdgeCode,
            StartNodeId = request.StartNodeId,
            EndNodeId = request.EndNodeId,
            EdgeType = request.EdgeType,
            IsBidirectional = request.IsBidirectional,
            ArcViaX = request.ArcViaX,
            ArcViaY = request.ArcViaY,
            Curvature = request.Curvature,
            Distance = distance
        };

        edge.OnCreate();

        await _edgeRepository.AddAsync(edge);
        await _edgeRepository.SaveChangesAsync();

        _logger.LogInformation("创建边成功: {EdgeCode} in Map {MapId}", edge.EdgeCode, mapId);

        var dto = edge.MapTo<MapEdgeListItemDto>();
        dto.StartNodeCode = startNode.NodeCode;
        dto.StartNodeName = startNode.DisplayName;
        dto.EndNodeCode = endNode.NodeCode;
        dto.EndNodeName = endNode.DisplayName;

        return Ok(ApiResponse<MapEdgeListItemDto>.Ok(dto, "创建成功"));
    }

    /// <summary>
    /// 更新边
    /// </summary>
    [HttpPut("{mapId:guid}/edges/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<MapEdgeListItemDto>>> UpdateEdge(Guid mapId, Guid id, [FromBody] UpdateMapEdgeRequest request)
    {
        var spec = new MapEdgeByIdSpec(id);
        var edge = await _edgeRepository.FirstOrDefaultAsync(spec);

        if (edge == null || edge.MapId != mapId)
        {
            return NotFound(ApiResponse<MapEdgeListItemDto>.Fail("边不存在"));
        }

        // 验证起点和终点节点存在
        var startNodeSpec = new MapNodeByIdSpec(request.StartNodeId);
        var startNode = await _nodeRepository.FirstOrDefaultAsync(startNodeSpec);
        if (startNode == null || startNode.MapId != mapId)
        {
            return BadRequest(ApiResponse<MapEdgeListItemDto>.Fail("起点节点不存在"));
        }

        var endNodeSpec = new MapNodeByIdSpec(request.EndNodeId);
        var endNode = await _nodeRepository.FirstOrDefaultAsync(endNodeSpec);
        if (endNode == null || endNode.MapId != mapId)
        {
            return BadRequest(ApiResponse<MapEdgeListItemDto>.Fail("终点节点不存在"));
        }

        if (request.StartNodeId == request.EndNodeId)
        {
            return BadRequest(ApiResponse<MapEdgeListItemDto>.Fail("起点和终点不能相同"));
        }

        // 用于计算距离的请求对象
        var createRequest = new CreateMapEdgeRequest
        {
            EdgeType = request.EdgeType,
            ArcViaX = request.ArcViaX,
            ArcViaY = request.ArcViaY,
            Curvature = request.Curvature
        };

        // 更新边的所有属性
        edge.StartNodeId = request.StartNodeId;
        edge.EndNodeId = request.EndNodeId;
        edge.EdgeType = request.EdgeType;
        edge.IsBidirectional = request.IsBidirectional;
        edge.ArcViaX = request.ArcViaX;
        edge.ArcViaY = request.ArcViaY;
        edge.Curvature = request.Curvature;
        edge.Distance = CalculateDistance(startNode, endNode, createRequest);

        edge.OnUpdate();

        await _edgeRepository.UpdateAsync(edge);
        await _edgeRepository.SaveChangesAsync();

        _logger.LogInformation("更新边成功: {EdgeCode}", edge.EdgeCode);

        var dto = edge.MapTo<MapEdgeListItemDto>();
        dto.StartNodeCode = startNode.NodeCode;
        dto.StartNodeName = startNode.DisplayName;
        dto.EndNodeCode = endNode.NodeCode;
        dto.EndNodeName = endNode.DisplayName;

        return Ok(ApiResponse<MapEdgeListItemDto>.Ok(dto, "更新成功"));
    }

    /// <summary>
    /// 删除边
    /// </summary>
    [HttpDelete("{mapId:guid}/edges/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteEdge(Guid mapId, Guid id)
    {
        var spec = new MapEdgeByIdSpec(id);
        var edge = await _edgeRepository.FirstOrDefaultAsync(spec);

        if (edge == null || edge.MapId != mapId)
        {
            return NotFound(ApiResponse<bool>.Fail("边不存在"));
        }

        // 检查是否有路线段引用此边
        // var segmentSpec = new Business.Specifications.RouteSegments.RouteSegmentByEdgeIdSpec(id); // 旧RouteSegment已删除
        // var hasSegments = await _edgeRepository.AnyAsync(new MapEdgeByIdSpec(id));
        // Note: We need RouteSegment repository for this check
        // For now, we'll allow deletion

        edge.OnDelete("用户删除");

        await _edgeRepository.UpdateAsync(edge);
        await _edgeRepository.SaveChangesAsync();

        _logger.LogInformation("删除边成功: {EdgeCode}", edge.EdgeCode);

        return Ok(ApiResponse<bool>.Ok(true, "删除成功"));
    }

    /// <summary>
    /// 获取下一个可用的边编号
    /// </summary>
    [HttpGet("{mapId:guid}/edges/next-code")]
    public async Task<ActionResult<ApiResponse<string>>> GetNextEdgeCode(Guid mapId)
    {
        var spec = new MapEdgeMaxCodeSpec(mapId);
        var edge = await _edgeRepository.FirstOrDefaultAsync(spec);

        int nextSeq = 1;
        if (edge != null)
        {
            var seq = EntityCodes.ParseSequence(edge.EdgeCode, EntityCodes.EdgePrefix);
            if (seq.HasValue)
            {
                nextSeq = seq.Value + 1;
            }
        }

        var nextCode = EntityCodes.Generate(EntityCodes.EdgePrefix, nextSeq);
        return Ok(ApiResponse<string>.Ok(nextCode));
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 计算边的距离
    /// </summary>
    private static decimal CalculateDistance(MapNode startNode, MapNode endNode, CreateMapEdgeRequest request)
    {
        if (request.EdgeType == Shared.Enums.EdgeType.Arc && request.ArcViaX.HasValue && request.ArcViaY.HasValue)
        {
            // 弧线（经过点）：计算两段直线距离之和
            var d1 = CalculateLineDistance(startNode.X, startNode.Y, request.ArcViaX.Value, request.ArcViaY.Value);
            var d2 = CalculateLineDistance(request.ArcViaX.Value, request.ArcViaY.Value, endNode.X, endNode.Y);
            return d1 + d2;
        }
        else if (request.EdgeType == Shared.Enums.EdgeType.ArcWithCurvature && request.Curvature.HasValue)
        {
            // 弧线（曲率）：使用二次贝塞尔曲线长度近似
            // 计算控制点
            var (ctrlX, ctrlY) = CalculateCurvatureControlPoint(
                (float)startNode.X, (float)startNode.Y,
                (float)endNode.X, (float)endNode.Y,
                (float)request.Curvature.Value);

            // 使用折线长度近似曲线长度
            var d1 = CalculateLineDistance(startNode.X, startNode.Y, (decimal)ctrlX, (decimal)ctrlY);
            var d2 = CalculateLineDistance((decimal)ctrlX, (decimal)ctrlY, endNode.X, endNode.Y);
            return d1 + d2;
        }

        // 直线：两点间欧几里得距离
        return CalculateLineDistance(startNode.X, startNode.Y, endNode.X, endNode.Y);
    }

    /// <summary>
    /// 计算两点间直线距离
    /// </summary>
    private static decimal CalculateLineDistance(decimal x1, decimal y1, decimal x2, decimal y2)
    {
        var dx = (double)(x2 - x1);
        var dy = (double)(y2 - y1);
        return (decimal)Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 根据曲率值计算贝塞尔曲线的控制点
    /// </summary>
    private static (float ctrlX, float ctrlY) CalculateCurvatureControlPoint(float x1, float y1, float x2, float y2, float curvature)
    {
        // 计算中点
        var midX = (x1 + x2) / 2;
        var midY = (y1 + y2) / 2;

        // 计算线段长度
        var dx = x2 - x1;
        var dy = y2 - y1;
        var distance = (float)Math.Sqrt(dx * dx + dy * dy);

        // 计算垂直于线段的方向（顺时针旋转90度）
        var perpX = dy;
        var perpY = -dx;

        // 归一化垂直向量
        var perpLength = (float)Math.Sqrt(perpX * perpX + perpY * perpY);
        if (perpLength > 0)
        {
            perpX /= perpLength;
            perpY /= perpLength;
        }

        // 根据曲率值计算偏移距离
        var offset = curvature * distance * 0.5f;

        // 计算控制点
        var ctrlX = midX + perpX * offset;
        var ctrlY = midY + perpY * offset;

        return (ctrlX, ctrlY);
    }

    #endregion
}
