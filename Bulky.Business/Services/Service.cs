using Bulky.Business.Abstract;
using Bulky.DataAccess.Repository.IRepositories;
using System.Linq.Expressions;

namespace Bulky.Business.Services
{
    public class Service<T> : IService<T> where T : class
    {
        private readonly IRepository<T> _repository;

        public Service(IRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(T entity)
        {
            await _repository.AddAsync(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            return await _repository.GetAllAsync(filter, includeProperties);
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            return await _repository.GetAsync(filter, includeProperties);
        }

        public void Remove(T entity)
        {
            _repository.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _repository.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _repository.Update(entity);
        }
    }
}