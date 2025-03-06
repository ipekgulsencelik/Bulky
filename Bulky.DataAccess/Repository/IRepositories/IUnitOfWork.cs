namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        ICompanyRepository Companies { get; }
        IShoppingCartRepository ShoppingCarts { get; }
        IApplicationUserRepository ApplicationUsers { get; }
        IOrderDetailRepository OrderDetails { get; }
        IOrderRepository Orders { get; }

        Task CommitAsync();
    }
}