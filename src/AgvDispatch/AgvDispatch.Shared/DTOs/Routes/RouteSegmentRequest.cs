using System.ComponentModel.DataAnnotations;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Shared.DTOs.Routes;

/// <summary>
/// 路线段请求
/// </summary>
public class RouteSegmentRequest
{
    [Required(ErrorMessage = "边ID不能为空")]
    public Guid EdgeId { get; set; }

    public int Seq { get; set; }

    public DriveDirection Direction { get; set; } = DriveDirection.Forward;

    public FinalAction Action { get; set; } = FinalAction.None;

    [Range(0, 3600, ErrorMessage = "等待时间必须在0到3600秒之间")]
    public int WaitTime { get; set; }
}
