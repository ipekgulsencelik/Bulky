using Bulky.DataAccess.Context;
using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.Repositories
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private BulkyContext _db;

        public ShoppingCartRepository(BulkyContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }
    }
}