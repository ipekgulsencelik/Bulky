using Bulky.Entity.Entities;

namespace Bulky.UI.Models
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public Order Order { get; set; }
    }
}