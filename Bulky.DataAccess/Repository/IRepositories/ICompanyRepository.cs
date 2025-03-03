using Bulky.Entity.Entities;

namespace Bulky.DataAccess.Repository.IRepositories
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company obj);
    }
}