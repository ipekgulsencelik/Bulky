using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        void Update(OrderDetail obj);
    }
}