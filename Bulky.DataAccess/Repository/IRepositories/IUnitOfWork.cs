﻿namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }

        Task CommitAsync();
    }
}