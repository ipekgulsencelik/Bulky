using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        public void Update(ApplicationUser user);
    }
}