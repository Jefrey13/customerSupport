using System.Linq.Expressions;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellation = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellation = default);
        Task AddAsync(T entity, CancellationToken cancellation = default);
        void Update(T entity);
        void Remove(T entity);
    }
}
