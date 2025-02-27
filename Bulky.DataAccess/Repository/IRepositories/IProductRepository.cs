using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj);
    }
}