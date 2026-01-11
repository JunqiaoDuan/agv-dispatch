using AgvDispatch.Business.Entities.MapAggregate;
using AgvDispatch.Business.Entities.RouteAggregate;
using AgvDispatch.Business.Specifications.MapEdges;
using RouteEntity = AgvDispatch.Business.Entities.RouteAggregate.Route;
using AgvDispatch.Business.Specifications.MapNodes;
using AgvDispatch.Business.Specifications.Maps;
using AgvDispatch.Business.Specifications.Routes;
using AgvDispatch.Business.Specifications.RouteSegments;
using AgvDispatch.Shared.Constants;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Routes;
using AgvDispatch.Shared.Extensions;
using AgvDispatch.Shared.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgvDispatch.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoutesController : ControllerBase
{
    private readonly IRepository<RouteEntity> _routeRepository;
    private readonly IRepository<RouteSegment> _segmentRepository;
    private readonly IRepository<Map> _mapRepository;
    private readonly IRepository<MapNode> _nodeRepository;
    private readonly IRepository<MapEdge> _edgeRepository;
    private readonly ILogger<RoutesController> _logger;

    public RoutesController(
        IRepository<RouteEntity> routeRepository,
        IRepository<RouteSegment> segmentRepository,
        IRepository<Map> mapRepository,
        IRepository<MapNode> nodeRepository,
        IRepository<MapEdge> edgeRepository,
        ILogger<RoutesController> logger)
    {
        _routeRepository = routeRepository;
        _segmentRepository = segmentRepository;
        _mapRepository = mapRepository;
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取路线列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RouteListItemDto>>>> GetAll([FromQuery] Guid mapId)
    {
        var spec = new RouteListSpec(mapId);
        var routes = await _routeRepository.ListAsync(spec);

        // 获取所有地图用于显示名称
        var mapSpec = new MapListSpec();
        var maps = await _mapRepository.ListAsync(mapSpec);
        var mapDict = maps.ToDictionary(x => x.Id, x => x.DisplayName);

        var items = new List<RouteListItemDto>();
        foreach (var route in routes)
        {
            var segmentCount = await _segmentRepository.CountAsync(new RouteSegmentCountSpec(route.Id));
            var dto = route.MapTo<RouteListItemDto>();
            dto.SegmentCount = segmentCount;
            dto.MapName = mapDict.TryGetValue(route.MapId, out var name) ? name : "";
            items.Add(dto);
        }

        return Ok(ApiResponse<List<RouteListItemDto>>.Ok(items));
    }

    /// <summary>
    /// 获取路线详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RouteDetailDto>>> GetById(Guid id)
    {
        var spec = new RouteByIdSpec(id);
        var route = await _routeRepository.FirstOrDefaultAsync(spec);

        if (route == null)
        {
            return NotFound(ApiResponse<RouteDetailDto>.Fail("路线不存在"));
        }

        // 获取地图名称
        var mapSpec = new MapByIdSpec(route.MapId);
        var map = await _mapRepository.FirstOrDefaultAsync(mapSpec);

        // 获取路线段
        var segmentSpec = new RouteSegmentListSpec(route.Id);
        var segments = await _segmentRepository.ListAsync(segmentSpec);

        // 获取边和节点信息
        var edgeSpec = new MapEdgeListSpec(route.MapId);
        var edges = await _edgeRepository.ListAsync(edgeSpec);
        var edgeDict = edges.ToDictionary(x => x.Id, x => x);

        var nodeSpec = new MapNodeListSpec(route.MapId);
        var nodes = await _nodeRepository.ListAsync(nodeSpec);
        var nodeDict = nodes.ToDictionary(x => x.Id, x => x);

        var dto = route.MapTo<RouteDetailDto>();
        dto.MapName = map?.DisplayName ?? "";
        dto.Segments = segments.Select(seg =>
        {
            var segDto = seg.MapTo<RouteSegmentDto>();
            if (edgeDict.TryGetValue(seg.EdgeId, out var edge))
            {
                segDto.EdgeCode = edge.EdgeCode;
                if (nodeDict.TryGetValue(edge.StartNodeId, out var startNode))
                {
                    segDto.StartNodeCode = startNode.NodeCode;
                    segDto.StartNodeName = startNode.DisplayName;
                }
                if (nodeDict.TryGetValue(edge.EndNodeId, out var endNode))
                {
                    segDto.EndNodeCode = endNode.NodeCode;
                    segDto.EndNodeName = endNode.DisplayName;
                }
            }
            return segDto;
        }).ToList();

        return Ok(ApiResponse<RouteDetailDto>.Ok(dto));
    }

    /// <summary>
    /// 创建路线
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<RouteDetailDto>>> Create([FromBody] CreateRouteRequest request)
    {
        // 验证地图存在
        var mapSpec = new MapByIdSpec(request.MapId);
        var map = await _mapRepository.FirstOrDefaultAsync(mapSpec);
        if (map == null)
        {
            return BadRequest(ApiResponse<RouteDetailDto>.Fail("所属地图不存在"));
        }

        // 验证编号唯一性
        var codeSpec = new RouteCodeExistsSpec(request.MapId, request.RouteCode);
        var exists = await _routeRepository.AnyAsync(codeSpec);
        if (exists)
        {
            return BadRequest(ApiResponse<RouteDetailDto>.Fail($"路线编号 {request.RouteCode} 已存在"));
        }

        // 创建路线
        var route = new RouteEntity
        {
            MapId = request.MapId,
            RouteCode = request.RouteCode,
            DisplayName = request.DisplayName,
            Description = request.Description,
            SortNo = request.SortNo,
            IsActive = true
        };

        route.OnCreate();

        await _routeRepository.AddAsync(route);
        await _routeRepository.SaveChangesAsync();

        // 创建路线段
        if (request.Segments.Count > 0)
        {
            foreach (var segReq in request.Segments)
            {
                var segment = new RouteSegment
                {
                    RouteId = route.Id,
                    EdgeId = segReq.EdgeId,
                    Seq = segReq.Seq,
                    Direction = segReq.Direction,
                    Action = segReq.Action,
                    WaitTime = segReq.WaitTime
                };
                segment.OnCreate();
                await _segmentRepository.AddAsync(segment);
            }
            await _segmentRepository.SaveChangesAsync();
        }

        _logger.LogInformation("创建路线成功: {RouteCode}", route.RouteCode);

        // 返回详情
        return await GetById(route.Id);
    }

    /// <summary>
    /// 更新路线
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<RouteDetailDto>>> Update(Guid id, [FromBody] UpdateRouteRequest request)
    {
        var spec = new RouteByIdSpec(id);
        var route = await _routeRepository.FirstOrDefaultAsync(spec);

        if (route == null)
        {
            return NotFound(ApiResponse<RouteDetailDto>.Fail("路线不存在"));
        }

        route.DisplayName = request.DisplayName;
        route.Description = request.Description;
        route.IsActive = request.IsActive;
        route.SortNo = request.SortNo;

        route.OnUpdate();

        await _routeRepository.UpdateAsync(route);
        await _routeRepository.SaveChangesAsync();

        // 更新路线段：先软删除旧的，再创建新的
        var oldSegmentSpec = new RouteSegmentListSpec(route.Id);
        var oldSegments = await _segmentRepository.ListAsync(oldSegmentSpec);
        foreach (var oldSeg in oldSegments)
        {
            oldSeg.OnDelete("更新路线段");
            await _segmentRepository.UpdateAsync(oldSeg);
        }
        await _segmentRepository.SaveChangesAsync();

        // 创建新的路线段
        if (request.Segments.Count > 0)
        {
            foreach (var segReq in request.Segments)
            {
                var segment = new RouteSegment
                {
                    RouteId = route.Id,
                    EdgeId = segReq.EdgeId,
                    Seq = segReq.Seq,
                    Direction = segReq.Direction,
                    Action = segReq.Action,
                    WaitTime = segReq.WaitTime
                };
                segment.OnCreate();
                await _segmentRepository.AddAsync(segment);
            }
            await _segmentRepository.SaveChangesAsync();
        }

        _logger.LogInformation("更新路线成功: {RouteCode}", route.RouteCode);

        // 返回详情
        return await GetById(route.Id);
    }

    /// <summary>
    /// 删除路线（软删除）
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var spec = new RouteByIdSpec(id);
        var route = await _routeRepository.FirstOrDefaultAsync(spec);

        if (route == null)
        {
            return NotFound(ApiResponse<bool>.Fail("路线不存在"));
        }

        // 软删除路线段
        var segmentSpec = new RouteSegmentListSpec(route.Id);
        var segments = await _segmentRepository.ListAsync(segmentSpec);
        foreach (var seg in segments)
        {
            seg.OnDelete("删除路线");
            await _segmentRepository.UpdateAsync(seg);
        }
        await _segmentRepository.SaveChangesAsync();

        // 软删除路线
        route.OnDelete("用户删除");

        await _routeRepository.UpdateAsync(route);
        await _routeRepository.SaveChangesAsync();

        _logger.LogInformation("删除路线成功: {RouteCode}", route.RouteCode);

        return Ok(ApiResponse<bool>.Ok(true, "删除成功"));
    }

    /// <summary>
    /// 获取下一个可用的路线编号
    /// </summary>
    [HttpGet("next-code")]
    public async Task<ActionResult<ApiResponse<string>>> GetNextCode([FromQuery] Guid mapId)
    {
        var spec = new RouteMaxCodeSpec(mapId);
        var route = await _routeRepository.FirstOrDefaultAsync(spec);

        int nextSeq = 1;
        if (route != null)
        {
            var seq = EntityCodes.ParseSequence(route.RouteCode, EntityCodes.RoutePrefix);
            if (seq.HasValue)
            {
                nextSeq = seq.Value + 1;
            }
        }

        var nextCode = EntityCodes.Generate(EntityCodes.RoutePrefix, nextSeq);
        return Ok(ApiResponse<string>.Ok(nextCode));
    }
}
