using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Agvs;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Extensions;
using AgvDispatch.Shared.Repository;
using Microsoft.AspNetCore.Mvc;

namespace AgvDispatch.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgvsController : ControllerBase
{
    private readonly IRepository<Agv> _agvRepository;
    private readonly ILogger<AgvsController> _logger;

    public AgvsController(IRepository<Agv> agvRepository, ILogger<AgvsController> logger)
    {
        _agvRepository = agvRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有小车列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AgvListItemDto>>>> GetAll()
    {
        var spec = new AgvListSpec();
        var agvs = await _agvRepository.ListAsync(spec);
        var items = agvs.Select(x => x.MapTo<AgvListItemDto>()).ToList();

        return Ok(ApiResponse<List<AgvListItemDto>>.Ok(items));
    }

    /// <summary>
    /// 获取小车分页列表
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<AgvListItemDto>>>> GetPaged([FromQuery] PagedAgvRequest request)
    {
        // 获取总数
        var countSpec = new AgvCountSpec(request.SearchText);
        var totalCount = await _agvRepository.CountAsync(countSpec);

        // 分页查询
        var pagedSpec = new AgvPagedSpec(
            request.SearchText,
            request.SortBy,
            request.SortDescending,
            request.PageIndex,
            request.PageSize);

        var agvs = await _agvRepository.ListAsync(pagedSpec);
        var items = agvs.Select(x => x.MapTo<AgvListItemDto>()).ToList();

        var response = new PagedResponse<AgvListItemDto>
        {
            Items = items,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<PagedResponse<AgvListItemDto>>.Ok(response));
    }

    /// <summary>
    /// 获取小车详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AgvDetailDto>>> GetById(Guid id)
    {
        var spec = new AgvByIdSpec(id);
        var agv = await _agvRepository.FirstOrDefaultAsync(spec);

        if (agv == null)
        {
            return NotFound(ApiResponse<AgvDetailDto>.Fail("小车不存在"));
        }

        var dto = agv.MapTo<AgvDetailDto>();

        return Ok(ApiResponse<AgvDetailDto>.Ok(dto));
    }

    /// <summary>
    /// 创建小车
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AgvDetailDto>>> Create([FromBody] CreateAgvRequest request)
    {
        // 验证编号唯一性
        var codeSpec = new AgvCodeExistsSpec(request.AgvCode);
        var exists = await _agvRepository.AnyAsync(codeSpec);

        if (exists)
        {
            return BadRequest(ApiResponse<AgvDetailDto>.Fail($"小车编号 {request.AgvCode} 已存在"));
        }

        var agv = new Agv
        {
            Id = Guid.NewGuid(),
            AgvCode = request.AgvCode,
            DisplayName = request.DisplayName,
            SortNo = request.SortNo,
            Description = request.Description,
            AgvStatus = AgvStatus.Offline,
            Battery = 100
        };

        agv.SetPassword(request.Password);
        agv.OnCreate();

        await _agvRepository.AddAsync(agv);
        await _agvRepository.SaveChangesAsync();

        _logger.LogInformation("创建小车成功: {AgvCode}", agv.AgvCode);

        var dto = agv.MapTo<AgvDetailDto>();

        return CreatedAtAction(nameof(GetById), new { id = agv.Id }, ApiResponse<AgvDetailDto>.Ok(dto, "创建成功"));
    }

    /// <summary>
    /// 更新小车
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AgvDetailDto>>> Update(Guid id, [FromBody] UpdateAgvRequest request)
    {
        var spec = new AgvByIdSpec(id);
        var agv = await _agvRepository.FirstOrDefaultAsync(spec);

        if (agv == null)
        {
            return NotFound(ApiResponse<AgvDetailDto>.Fail("小车不存在"));
        }

        agv.DisplayName = request.DisplayName;
        agv.SortNo = request.SortNo;
        agv.Description = request.Description;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            agv.SetPassword(request.NewPassword);
        }

        agv.OnUpdate();

        await _agvRepository.UpdateAsync(agv);
        await _agvRepository.SaveChangesAsync();

        _logger.LogInformation("更新小车成功: {AgvCode}", agv.AgvCode);

        var dto = agv.MapTo<AgvDetailDto>();

        return Ok(ApiResponse<AgvDetailDto>.Ok(dto, "更新成功"));
    }

    /// <summary>
    /// 删除小车（软删除）
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var spec = new AgvByIdSpec(id);
        var agv = await _agvRepository.FirstOrDefaultAsync(spec);

        if (agv == null)
        {
            return NotFound(ApiResponse<bool>.Fail("小车不存在"));
        }

        agv.OnDelete("用户删除");

        await _agvRepository.UpdateAsync(agv);
        await _agvRepository.SaveChangesAsync();

        _logger.LogInformation("删除小车成功: {AgvCode}", agv.AgvCode);

        return Ok(ApiResponse<bool>.Ok(true, "删除成功"));
    }

    /// <summary>
    /// 获取下一个可用的小车编号
    /// </summary>
    [HttpGet("next-code")]
    public async Task<ActionResult<ApiResponse<string>>> GetNextCode()
    {
        var spec = new AgvMaxCodeSpec();
        var agv = await _agvRepository.FirstOrDefaultAsync(spec);

        int nextSeq = 1;
        if (agv != null && agv.AgvCode.Length > 1)
        {
            if (int.TryParse(agv.AgvCode.Substring(1), out int currentSeq))
            {
                nextSeq = currentSeq + 1;
            }
        }

        var nextCode = $"V{nextSeq:D3}";
        return Ok(ApiResponse<string>.Ok(nextCode));
    }
}
