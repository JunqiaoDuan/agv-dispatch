using System.Reflection;

namespace AgvDispatch.Shared.Extensions;

/// <summary>
/// 对象映射扩展方法
/// </summary>
public static class ObjectMapperExtensions
{
    /// <summary>
    /// 将源对象映射到新的目标类型实例
    /// </summary>
    /// <typeparam name="TTarget">目标类型</typeparam>
    /// <param name="source">源对象</param>
    /// <returns>映射后的目标对象</returns>
    public static TTarget MapTo<TTarget>(this object source) where TTarget : new()
    {
        if (source == null)
            return default!;

        var target = new TTarget();
        CopyProperties(source, target);
        return target;
    }

    /// <summary>
    /// 将源对象的属性复制到已存在的目标对象
    /// </summary>
    /// <typeparam name="TTarget">目标类型</typeparam>
    /// <param name="source">源对象</param>
    /// <param name="target">目标对象</param>
    /// <returns>目标对象</returns>
    public static TTarget MapTo<TTarget>(this object source, TTarget target)
    {
        if (source == null || target == null)
            return target;

        CopyProperties(source, target);
        return target;
    }

    /// <summary>
    /// 将源集合映射为目标类型集合
    /// </summary>
    /// <typeparam name="TTarget">目标类型</typeparam>
    /// <param name="sources">源集合</param>
    /// <returns>映射后的目标集合</returns>
    public static List<TTarget> MapToList<TTarget>(this IEnumerable<object> sources) where TTarget : new()
    {
        return sources.Select(s => s.MapTo<TTarget>()).ToList();
    }

    private static void CopyProperties(object source, object target)
    {
        var sourceType = source.GetType();
        var targetType = target.GetType();

        var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var sourceProp in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!sourceProp.CanRead)
                continue;

            if (!targetProps.TryGetValue(sourceProp.Name, out var targetProp))
                continue;

            // 类型兼容检查
            if (!IsTypeCompatible(sourceProp.PropertyType, targetProp.PropertyType))
                continue;

            try
            {
                var value = sourceProp.GetValue(source);
                targetProp.SetValue(target, value);
            }
            catch
            {
                // 忽略赋值失败的属性
            }
        }
    }

    private static bool IsTypeCompatible(Type sourceType, Type targetType)
    {
        // 完全相同
        if (sourceType == targetType)
            return true;

        // 可空类型处理
        var underlyingSource = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
        var underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingSource == underlyingTarget)
            return true;

        // 目标类型可以从源类型赋值
        if (targetType.IsAssignableFrom(sourceType))
            return true;

        return false;
    }
}
