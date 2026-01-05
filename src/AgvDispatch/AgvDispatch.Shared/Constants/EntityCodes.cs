namespace AgvDispatch.Shared.Constants;

/// <summary>
/// 实体编号常量与生成器
/// </summary>
public static class EntityCodes
{
    /// <summary>
    /// AGV小车编号前缀
    /// </summary>
    public const string AgvPrefix = "V";

    /// <summary>
    /// 地图编号前缀
    /// </summary>
    public const string MapPrefix = "M";

    /// <summary>
    /// 地图节点编号前缀
    /// </summary>
    public const string NodePrefix = "N";

    /// <summary>
    /// 地图边编号前缀
    /// </summary>
    public const string EdgePrefix = "E";

    /// <summary>
    /// 路线编号前缀
    /// </summary>
    public const string RoutePrefix = "R";

    /// <summary>
    /// 站点编号前缀
    /// </summary>
    public const string StationPrefix = "S";

    /// <summary>
    /// 默认编号位数
    /// </summary>
    public const int DefaultDigits = 3;

    /// <summary>
    /// 生成编号
    /// </summary>
    /// <param name="prefix">前缀</param>
    /// <param name="sequence">序号</param>
    /// <param name="digits">数字位数，默认3位</param>
    /// <returns>格式化的编号，如 V001</returns>
    public static string Generate(string prefix, int sequence, int digits = DefaultDigits)
    {
        return $"{prefix}{sequence.ToString().PadLeft(digits, '0')}";
    }

    /// <summary>
    /// 从编号中解析序号
    /// </summary>
    /// <param name="code">编号，如 V001</param>
    /// <param name="prefix">前缀</param>
    /// <returns>序号，解析失败返回 null</returns>
    public static int? ParseSequence(string code, string prefix)
    {
        if (string.IsNullOrEmpty(code) || !code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;

        var numberPart = code[prefix.Length..];
        return int.TryParse(numberPart, out var sequence) ? sequence : null;
    }

}
