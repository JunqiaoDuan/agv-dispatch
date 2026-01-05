using Ardalis.Specification;

namespace AgvDispatch.Shared.Repository;

/// <summary>
/// 只读仓储接口
/// </summary>
public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class
{
}
