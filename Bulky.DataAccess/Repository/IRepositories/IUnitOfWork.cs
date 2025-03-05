﻿namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        ICompanyRepository Companies { get; }
        IShoppingCartRepository ShoppingCarts { get; }

        Task CommitAsync();
    }
}