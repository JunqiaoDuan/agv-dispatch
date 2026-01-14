namespace AgvDispatch.Shared.PathFinding;

/// <summary>
/// 路径查找结果
/// </summary>
public class PathfindingResult
{
    /// <summary>
    /// 是否找到路径
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 节点路径（按顺序）
    /// </summary>
    public List<Guid> NodePath { get; set; } = [];

    /// <summary>
    /// 边路径（按顺序）
    /// </summary>
    public List<Guid> EdgePath { get; set; } = [];

    /// <summary>
    /// 总距离（厘米）
    /// </summary>
    public decimal TotalDistance { get; set; }

    /// <summary>
    /// 错误消息（失败时）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static PathfindingResult CreateSuccess(List<Guid> nodePath, List<Guid> edgePath, decimal totalDistance)
    {
        return new PathfindingResult
        {
            Success = true,
            NodePath = nodePath,
            EdgePath = edgePath,
            TotalDistance = totalDistance
        };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static PathfindingResult CreateFailure(string errorMessage)
    {
        return new PathfindingResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
