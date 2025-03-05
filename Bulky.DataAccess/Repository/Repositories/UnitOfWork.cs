using Bulky.DataAccess.Context;
using Bulky.DataAccess.Repository.IRepositories;

namespace Bulky.DataAccess.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BulkyContext _db;

        public ICategoryRepository Categories { get; }
        public IProductRepository Products { get; }
        public ICompanyRepository Companies { get; }
        public IShoppingCartRepository ShoppingCarts { get; }

        public UnitOfWork(BulkyContext db, ICategoryRepository categoryRepository, IProductRepository productRepository, ICompanyRepository companyRepository, IShoppingCartRepository shoppingCartRepository)
        {
            _db = db;
            Categories = categoryRepository;
            Products = productRepository;
            Companies = companyRepository;
            ShoppingCarts = shoppingCartRepository;
        }

        public async Task CommitAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}