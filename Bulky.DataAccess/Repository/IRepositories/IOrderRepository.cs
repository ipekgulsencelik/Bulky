using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        void Update(Order obj);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
    }
}