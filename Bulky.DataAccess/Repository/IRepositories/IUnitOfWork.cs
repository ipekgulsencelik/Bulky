namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }

        Task CommitAsync();
    }
}