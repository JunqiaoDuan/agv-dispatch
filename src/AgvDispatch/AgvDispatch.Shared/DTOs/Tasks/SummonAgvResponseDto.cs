namespace AgvDispatch.Shared.DTOs.Tasks;

/// <summary>
/// 召唤小车响应 DTO
/// </summary>
public class SummonAgvResponseDto
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 推荐的小车列表(按评分从高到低排序)
    /// </summary>
    public List<AgvRecommendationDto> Recommendations { get; set; } = new();
}
