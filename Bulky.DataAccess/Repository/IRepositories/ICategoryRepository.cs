using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category obj);
    }
}