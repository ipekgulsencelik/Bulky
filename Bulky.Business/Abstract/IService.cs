using System.Linq.Expressions;

namespace Bulky.Business.Abstract
{
    public interface IService<T> where T : class
    {
        Task<IEnumerable<T>> TGetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        Task<T?> TGetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);
        Task TAddAsync(T entity);
        void TUpdate(T entity);
        void TRemove(T entity);
        void TRemoveRange(IEnumerable<T> entities);
    }
}