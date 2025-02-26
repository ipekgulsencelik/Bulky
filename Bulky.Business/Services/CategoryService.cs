using Bulky.Business.Abstract;
using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Entity.Entities;
using System.Linq.Expressions;

namespace Bulky.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task TAddAsync(Category entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Category>> TGetAllAsync(Expression<Func<Category, bool>>? filter = null, string? includeProperties = null)
        {
            throw new NotImplementedException();
        }

        public Task<Category?> TGetAsync(Expression<Func<Category, bool>> filter, string? includeProperties = null)
        {
            throw new NotImplementedException();
        }

        public void TRemove(Category entity)
        {
            throw new NotImplementedException();
        }

        public void TRemoveRange(IEnumerable<Category> entities)
        {
            throw new NotImplementedException();
        }

        public void TUpdate(Category entity)
        {
            throw new NotImplementedException();
        }
    }
}