using Bulky.DataAccess.Context;
using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.Repositories
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private BulkyContext _db;

        public OrderDetailRepository(BulkyContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}