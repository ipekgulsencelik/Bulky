using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, IEnumerable<string>? includeProperties = null);
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, IEnumerable<string>? includeProperties = null);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}