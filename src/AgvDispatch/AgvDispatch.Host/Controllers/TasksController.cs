using System.Security.Claims;
using AgvDispatch.Business.Entities.TaskAggregate;
using AgvDispatch.Business.Services;
using AgvDispatch.Business.Specifications.TaskJobs;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Tasks;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Extensions;
using AgvDispatch.Shared.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskStatusEnum = AgvDispatch.Shared.Enums.TaskJobStatus;

namespace AgvDispatch.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskJobService _taskJobService;
    private readonly IRepository<TaskJob> _taskRepository;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        ITaskJobService taskJobService,
        IRepository<TaskJob> taskRepository,
        ILogger<TasksController> logger)
    {
        _taskJobService = taskJobService;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    #region AGV推荐/待处理

    /// <summary>
    /// 获取AGV推荐列表
    /// </summary>
    [HttpPost("recommendations")]
    public async Task<ActionResult<ApiResponse<List<AgvRecommendationDto>>>> GetRecommendations(
        [FromBody] GetRecommendationsRequestDto request)
    {
        var recommendations = await _taskJobService.GetAgvRecommendationsAsync(request.TaskType);

        var dtoList = recommendations.MapToList<AgvRecommendationDto>();

        _logger.LogInformation("[TasksController] 获取推荐列表成功: 推荐数量={Count}", recommendations.Count);

        return Ok(ApiResponse<List<AgvRecommendationDto>>.Ok(dtoList,
            recommendations.Count > 0 ? "获取推荐列表成功" : "暂无可用小车"));
    }

    /// <summary>
    /// 获取等待下料的小车列表
    /// </summary>
    [HttpGet("pending-unloading-agvs")]
    public async Task<ActionResult<ApiResponse<List<AgvPendingItemDto>>>> GetPendingUnloadingAgvs()
    {
        var items = await _taskJobService.GetPendingUnloadingAgvsAsync();
        var dtoList = items.MapToList<AgvPendingItemDto>();

        _logger.LogInformation("[TasksController] 获取等待下料小车列表成功: 数量={Count}", items.Count);

        return Ok(ApiResponse<List<AgvPendingItemDto>>.Ok(dtoList,
            items.Count > 0 ? "获取成功" : "暂无小车"));
    }

    /// <summary>
    /// 获取等待返回的小车列表
    /// </summary>
    [HttpGet("pending-return-agvs")]
    public async Task<ActionResult<ApiResponse<List<AgvPendingItemDto>>>> GetPendingReturnAgvs()
    {
        var items = await _taskJobService.GetPendingReturnAgvsAsync();
        var dtoList = items.MapToList<AgvPendingItemDto>();

        _logger.LogInformation("[TasksController] 获取等待返回小车列表成功: 数量={Count}", items.Count);

        return Ok(ApiResponse<List<AgvPendingItemDto>>.Ok(dtoList,
            items.Count > 0 ? "获取成功" : "暂无小车"));
    }

    /// <summary>
    /// 获取可充电的小车列表
    /// </summary>
    [HttpGet("chargeable-agvs")]
    public async Task<ActionResult<ApiResponse<List<AgvPendingItemDto>>>> GetChargeableAgvs()
    {
        var items = await _taskJobService.GetChargeableAgvsAsync();
        var dtoList = items.MapToList<AgvPendingItemDto>();

        _logger.LogInformation("[TasksController] 获取可充电小车列表成功: 数量={Count}", items.Count);

        return Ok(ApiResponse<List<AgvPendingItemDto>>.Ok(dtoList,
            items.Count > 0 ? "获取成功" : "暂无小车"));
    }

    #endregion

    #region 任务CRUD

    /// <summary>
    /// 创建任务并分配AGV
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<CreateTaskResponseDto>>> CreateTask(
        [FromBody] CreateTaskWithAgvRequestDto request)
    {
        var userId = GetCurrentUserId();

        var task = await _taskJobService.CreateTaskWithAgvAsync(
            request.TaskType,
            request.TargetStationCode,
            request.SelectedAgvCode,
            userId);

        var response = new CreateTaskResponseDto
        {
            TaskId = task.Id
        };

        _logger.LogInformation("[TasksController] 创建任务成功: TaskId={TaskId}, AgvCode={AgvCode}",
            task.Id, request.SelectedAgvCode);

        return Ok(ApiResponse<CreateTaskResponseDto>.Ok(response, "任务创建成功，已下发"));
    }

    /// <summary>
    /// 取消任务
    /// </summary>
    [HttpPost("cancel")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelTask([FromBody] CancelTaskRequestDto request)
    {
        var userId = GetCurrentUserId();

        var (success, message) = await _taskJobService.CancelTaskAsync(
            request.TaskId,
            request.Reason,
            userId);

        if (!success)
        {
            return BadRequest(ApiResponse<bool>.Fail(message ?? "取消失败"));
        }

        _logger.LogInformation("[TasksController] 取消任务成功: TaskId={TaskId}, Reason={Reason}",
            request.TaskId, request.Reason);

        return Ok(ApiResponse<bool>.Ok(true, "取消成功"));
    }

    /// <summary>
    /// 根据状态获取任务列表
    /// </summary>
    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<ApiResponse<List<TaskListItemDto>>>> GetByStatus(TaskStatusEnum status)
    {
        var spec = new TaskByStatusSpec(status);
        var tasks = await _taskRepository.ListAsync(spec);
        var items = tasks.MapToList<TaskListItemDto>();

        return Ok(ApiResponse<List<TaskListItemDto>>.Ok(items));
    }

    /// <summary>
    /// 获取活动任务列表（待分配、已分配、执行中）
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<TaskListItemDto>>>> GetActiveTasks()
    {
        var spec = new TaskActiveSpec();
        var tasks = await _taskRepository.ListAsync(spec);
        var items = tasks.MapToList<TaskListItemDto>();

        return Ok(ApiResponse<List<TaskListItemDto>>.Ok(items));
    }

    /// <summary>
    /// 获取任务详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskDetailDto>>> GetById(Guid id)
    {
        var spec = new TaskByIdSpec(id);
        var task = await _taskRepository.FirstOrDefaultAsync(spec);

        if (task == null)
        {
            return NotFound(ApiResponse<TaskDetailDto>.Fail("任务不存在"));
        }

        var dto = task.MapTo<TaskDetailDto>();

        return Ok(ApiResponse<TaskDetailDto>.Ok(dto));
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 获取当前登录用户ID
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }

    /// <summary>
    /// 获取当前登录用户名
    /// </summary>
    private string? GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value;
    }

    #endregion
}
