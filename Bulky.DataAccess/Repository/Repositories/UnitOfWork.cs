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
        public IApplicationUserRepository ApplicationUsers { get; }
        public IOrderRepository Orders { get; }
        public IOrderDetailRepository OrderDetails { get; private set; }

        public UnitOfWork(BulkyContext db, ICategoryRepository categoryRepository, IProductRepository productRepository, ICompanyRepository companyRepository, IShoppingCartRepository shoppingCartRepository, IApplicationUserRepository applicationUserRepository, IOrderDetailRepository orderDetailRepository, IOrderRepository orderRepository)
        {
            _db = db;
            Categories = categoryRepository;
            Products = productRepository;
            Companies = companyRepository;
            ShoppingCarts = shoppingCartRepository;
            ApplicationUsers = applicationUserRepository;
            Orders = orderRepository;
            OrderDetails = orderDetailRepository;
        }

        public async Task CommitAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}