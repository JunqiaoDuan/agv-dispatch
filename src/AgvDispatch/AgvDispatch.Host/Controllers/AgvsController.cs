using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Business.Specifications.AgvExceptions;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Agvs;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Extensions;
using AgvDispatch.Shared.Repository;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgvDispatch.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgvsController : ControllerBase
{
    private readonly IRepository<Agv> _agvRepository;
    private readonly IRepository<AgvExceptionLog> _exceptionLogRepository;
    private readonly ILogger<AgvsController> _logger;

    public AgvsController(
        IRepository<Agv> agvRepository,
        IRepository<AgvExceptionLog> exceptionLogRepository,
        ILogger<AgvsController> logger)
    {
        _agvRepository = agvRepository;
        _exceptionLogRepository = exceptionLogRepository;
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
    [Authorize(Policy = "AdminOnly")]
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
            Id = NewId.NextSequentialGuid(),
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
    [Authorize(Policy = "AdminOnly")]
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
    [Authorize(Policy = "AdminOnly")]
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

    /// <summary>
    /// 获取AGV监控列表（包含异常统计）
    /// </summary>
    [HttpGet("monitor")]
    public async Task<ActionResult<ApiResponse<List<AgvMonitorItemDto>>>> GetMonitorList()
    {
        var spec = new AgvListSpec();
        var agvs = await _agvRepository.ListAsync(spec);

        var monitorItems = new List<AgvMonitorItemDto>();

        foreach (var agv in agvs)
        {
            var item = agv.MapTo<AgvMonitorItemDto>();

            // 查询未解决异常数量
            item.UnresolvedExceptionCount = await _exceptionLogRepository.CountAsync(
                new AgvUnresolvedExceptionCountSpec(agv.AgvCode));

            monitorItems.Add(item);
        }

        return Ok(ApiResponse<List<AgvMonitorItemDto>>.Ok(monitorItems));
    }

    /// <summary>
    /// 获取指定AGV的所有未解决异常
    /// </summary>
    [HttpGet("{agvCode}/exceptions/unresolved")]
    public async Task<ActionResult<ApiResponse<List<AgvExceptionSummaryDto>>>> GetAgvUnresolvedExceptions(string agvCode)
    {
        var spec = new AgvAllUnresolvedExceptionsSpec(agvCode);
        var unresolvedExceptions = await _exceptionLogRepository.ListAsync(spec);

        var items = unresolvedExceptions.Select(ex => new AgvExceptionSummaryDto
        {
            Id = ex.Id,
            ExceptionType = ex.ExceptionType,
            Severity = ex.Severity,
            Message = ex.Message,
            ExceptionTime = ex.ExceptionTime,
            StationCode = ex.StationCode
        }).ToList();

        return Ok(ApiResponse<List<AgvExceptionSummaryDto>>.Ok(items));
    }

    /// <summary>
    /// 获取指定AGV的所有异常（包括已解决的）- 分页查询
    /// </summary>
    [HttpGet("{agvCode}/exceptions/all")]
    public async Task<ActionResult<ApiResponse<PagedResponse<AgvExceptionSummaryDto>>>> GetAllAgvExceptions(
        string agvCode,
        [FromQuery] PagedAgvExceptionRequest request)
    {
        // 获取总数
        var countSpec = new AgvExceptionsCountSpec(agvCode, request.OnlyUnresolved, request.Severity);
        var totalCount = await _exceptionLogRepository.CountAsync(countSpec);

        // 分页查询
        var pagedSpec = new AgvExceptionsPagedSpec(
            agvCode,
            request.OnlyUnresolved,
            request.Severity,
            request.SortBy,
            request.SortDescending,
            request.PageIndex,
            request.PageSize);

        var exceptions = await _exceptionLogRepository.ListAsync(pagedSpec);

        var items = exceptions.Select(ex => new AgvExceptionSummaryDto
        {
            Id = ex.Id,
            ExceptionType = ex.ExceptionType,
            Severity = ex.Severity,
            Message = ex.Message,
            ExceptionTime = ex.ExceptionTime,
            StationCode = ex.StationCode,
            IsResolved = ex.IsResolved,
            ResolvedTime = ex.ResolvedTime
        }).ToList();

        var response = new PagedResponse<AgvExceptionSummaryDto>
        {
            Items = items,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<PagedResponse<AgvExceptionSummaryDto>>.Ok(response));
    }

    /// <summary>
    /// 批量解决异常
    /// </summary>
    [HttpPost("exceptions/resolve")]
    public async Task<ActionResult<ApiResponse<bool>>> ResolveExceptions([FromBody] List<Guid> exceptionIds)
    {
        if (exceptionIds == null || !exceptionIds.Any())
        {
            return BadRequest(ApiResponse<bool>.Fail("异常ID列表不能为空"));
        }

        var exceptions = await _exceptionLogRepository.ListAsync();
        var toUpdate = exceptions.Where(e => exceptionIds.Contains(e.Id) && !e.IsResolved).ToList();

        if (!toUpdate.Any())
        {
            return Ok(ApiResponse<bool>.Ok(true, "没有需要处理的异常"));
        }

        foreach (var exception in toUpdate)
        {
            exception.IsResolved = true;
            exception.ResolvedTime = DateTimeOffset.UtcNow;
            exception.ResolvedRemark = "手动消除";
            exception.OnUpdate();
        }

        await _exceptionLogRepository.UpdateRangeAsync(toUpdate);
        await _exceptionLogRepository.SaveChangesAsync();

        _logger.LogInformation("批量解决异常成功，共处理 {Count} 条异常", toUpdate.Count);

        return Ok(ApiResponse<bool>.Ok(true, $"成功处理 {toUpdate.Count} 条异常"));
    }
}
