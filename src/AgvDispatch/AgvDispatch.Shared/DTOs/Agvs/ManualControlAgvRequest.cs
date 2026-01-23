namespace AgvDispatch.Shared.DTOs.Agvs;

/// <summary>
/// 手动控制 AGV 货物状态请求
/// </summary>
public class ManualControlAgvRequest
{
    /// <summary>
    /// 是否有货
    /// </summary>
    public bool HasCargo { get; set; }

    /// <summary>
    /// 操作原因（必填）
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
