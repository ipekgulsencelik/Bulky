using Bulky.DataAccess.Context;
using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private BulkyContext _db;

        public OrderRepository(BulkyContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Order obj)
        {
            _db.Orders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.Orders.FirstOrDefault(x => x.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.Orders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}