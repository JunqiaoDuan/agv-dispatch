using Ardalis.Specification;

namespace AgvDispatch.Shared.Repository;

/// <summary>
/// 读写仓储接口
/// </summary>
public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
}
