using Bulky.DataAccess.Context;
using Bulky.DataAccess.Repository.IRepositories;

namespace Bulky.DataAccess.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BulkyContext _db;

        public ICategoryRepository Categories { get; }

        public UnitOfWork(BulkyContext db, ICategoryRepository categoryRepository)
        {
            _db = db;
            Categories = categoryRepository; 
        }

        public async Task CommitAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}