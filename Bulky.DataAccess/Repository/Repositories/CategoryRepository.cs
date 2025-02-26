using Bulky.DataAccess.Context;
using Bulky.DataAccess.Repository.IRepositories;
using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly BulkyContext _db;

        public CategoryRepository(BulkyContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }
    }
}