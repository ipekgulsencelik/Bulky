using Bulky.Entity.Entities;

namespace Bulky.UI.Models
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<OrderDetail> OrderDetail { get; set; }
    }
}