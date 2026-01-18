using AgvDispatch.Business.Entities.MapAggregate;
using AgvDispatch.Business.Entities.StationAggregate;
using AgvDispatch.Business.Specifications.MapNodes;
using AgvDispatch.Business.Specifications.Maps;
using AgvDispatch.Business.Specifications.Stations;
using AgvDispatch.Shared.Constants;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.Extensions;
using AgvDispatch.Shared.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgvDispatch.Host.Controllers;

[ApiController]
[Route("api/maps/{mapId:guid}/[controller]")]
[Authorize]
public class StationsController : ControllerBase
{
    private readonly IRepository<Station> _stationRepository;
    private readonly IRepository<Map> _mapRepository;
    private readonly IRepository<MapNode> _nodeRepository;
    private readonly ILogger<StationsController> _logger;

    public StationsController(
        IRepository<Station> stationRepository,
        IRepository<Map> mapRepository,
        IRepository<MapNode> nodeRepository,
        ILogger<StationsController> logger)
    {
        _stationRepository = stationRepository;
        _mapRepository = mapRepository;
        _nodeRepository = nodeRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取地图的所有站点列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StationListItemDto>>>> GetAll(Guid mapId)
    {
        var mapSpec = new MapByIdSpec(mapId);
        var mapExists = await _mapRepository.AnyAsync(mapSpec);
        if (!mapExists)
        {
            return NotFound(ApiResponse<List<StationListItemDto>>.Fail("地图不存在"));
        }

        var spec = new StationListSpec(mapId);
        var stations = await _stationRepository.ListAsync(spec);

        // 获取节点信息
        var nodeSpec = new MapNodeListSpec(mapId);
        var nodes = await _nodeRepository.ListAsync(nodeSpec);
        var nodeDict = nodes.ToDictionary(x => x.Id, x => x);

        var items = stations.Select(station =>
        {
            var dto = station.MapTo<StationListItemDto>();
            if (nodeDict.TryGetValue(station.NodeId, out var node))
            {
                dto.NodeCode = node.NodeCode;
                dto.NodeName = node.DisplayName;
                dto.X = node.X;
                dto.Y = node.Y;
            }
            return dto;
        }).ToList();

        return Ok(ApiResponse<List<StationListItemDto>>.Ok(items));
    }

    /// <summary>
    /// 获取站点详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StationListItemDto>>> GetById(Guid mapId, Guid id)
    {
        var spec = new StationByIdSpec(id);
        var station = await _stationRepository.FirstOrDefaultAsync(spec);

        if (station == null || station.MapId != mapId)
        {
            return NotFound(ApiResponse<StationListItemDto>.Fail("站点不存在"));
        }

        var dto = station.MapTo<StationListItemDto>();

        // 获取节点信息
        var nodeSpec = new MapNodeByIdSpec(station.NodeId);
        var node = await _nodeRepository.FirstOrDefaultAsync(nodeSpec);
        if (node != null)
        {
            dto.NodeCode = node.NodeCode;
            dto.NodeName = node.DisplayName;
            dto.X = node.X;
            dto.Y = node.Y;
        }

        return Ok(ApiResponse<StationListItemDto>.Ok(dto));
    }

    /// <summary>
    /// 创建站点
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<StationListItemDto>>> Create(Guid mapId, [FromBody] CreateStationRequest request)
    {
        var mapSpec = new MapByIdSpec(mapId);
        var mapExists = await _mapRepository.AnyAsync(mapSpec);
        if (!mapExists)
        {
            return NotFound(ApiResponse<StationListItemDto>.Fail("地图不存在"));
        }

        // 检查站点编号是否已存在
        var codeSpec = new StationCodeExistsSpec(mapId, request.StationCode);
        var exists = await _stationRepository.AnyAsync(codeSpec);
        if (exists)
        {
            return BadRequest(ApiResponse<StationListItemDto>.Fail($"站点编号 {request.StationCode} 已存在"));
        }

        // 验证节点存在
        var nodeSpec = new MapNodeByIdSpec(request.NodeId);
        var node = await _nodeRepository.FirstOrDefaultAsync(nodeSpec);
        if (node == null || node.MapId != mapId)
        {
            return BadRequest(ApiResponse<StationListItemDto>.Fail("关联的节点不存在"));
        }

        var station = new Station
        {
            MapId = mapId,
            NodeId = request.NodeId,
            StationCode = request.StationCode,
            DisplayName = request.DisplayName,
            StationType = request.StationType,
            Description = request.Description,
            SortNo = request.SortNo,
            Priority = request.Priority
        };

        station.OnCreate();

        await _stationRepository.AddAsync(station);
        await _stationRepository.SaveChangesAsync();

        _logger.LogInformation("创建站点成功: {StationCode} in Map {MapId}", station.StationCode, mapId);

        var dto = station.MapTo<StationListItemDto>();
        dto.NodeCode = node.NodeCode;
        dto.NodeName = node.DisplayName;
        dto.X = node.X;
        dto.Y = node.Y;

        return Ok(ApiResponse<StationListItemDto>.Ok(dto, "创建成功"));
    }

    /// <summary>
    /// 更新站点
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<StationListItemDto>>> Update(Guid mapId, Guid id, [FromBody] UpdateStationRequest request)
    {
        var spec = new StationByIdSpec(id);
        var station = await _stationRepository.FirstOrDefaultAsync(spec);

        if (station == null || station.MapId != mapId)
        {
            return NotFound(ApiResponse<StationListItemDto>.Fail("站点不存在"));
        }

        station.DisplayName = request.DisplayName;
        station.StationType = request.StationType;
        station.Description = request.Description;
        station.SortNo = request.SortNo;
        station.Priority = request.Priority;

        station.OnUpdate();

        await _stationRepository.UpdateAsync(station);
        await _stationRepository.SaveChangesAsync();

        _logger.LogInformation("更新站点成功: {StationCode}", station.StationCode);

        var dto = station.MapTo<StationListItemDto>();

        // 获取节点信息
        var nodeSpec = new MapNodeByIdSpec(station.NodeId);
        var node = await _nodeRepository.FirstOrDefaultAsync(nodeSpec);
        if (node != null)
        {
            dto.NodeCode = node.NodeCode;
            dto.NodeName = node.DisplayName;
            dto.X = node.X;
            dto.Y = node.Y;
        }

        return Ok(ApiResponse<StationListItemDto>.Ok(dto, "更新成功"));
    }

    /// <summary>
    /// 删除站点（软删除）
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid mapId, Guid id)
    {
        var spec = new StationByIdSpec(id);
        var station = await _stationRepository.FirstOrDefaultAsync(spec);

        if (station == null || station.MapId != mapId)
        {
            return NotFound(ApiResponse<bool>.Fail("站点不存在"));
        }

        station.OnDelete("用户删除");

        await _stationRepository.UpdateAsync(station);
        await _stationRepository.SaveChangesAsync();

        _logger.LogInformation("删除站点成功: {StationCode}", station.StationCode);

        return Ok(ApiResponse<bool>.Ok(true, "删除成功"));
    }

    /// <summary>
    /// 获取下一个可用的站点编号
    /// </summary>
    [HttpGet("next-code")]
    public async Task<ActionResult<ApiResponse<string>>> GetNextCode(Guid mapId)
    {
        var spec = new StationMaxCodeSpec(mapId);
        var station = await _stationRepository.FirstOrDefaultAsync(spec);

        int nextSeq = 1;
        if (station != null)
        {
            var seq = EntityCodes.ParseSequence(station.StationCode, EntityCodes.StationPrefix);
            if (seq.HasValue)
            {
                nextSeq = seq.Value + 1;
            }
        }

        var nextCode = EntityCodes.Generate(EntityCodes.StationPrefix, nextSeq);
        return Ok(ApiResponse<string>.Ok(nextCode));
    }
}
