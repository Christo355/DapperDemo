using DapperDemo.Models;

namespace DapperDemo.Repository
{
    public interface IBonusRepository
    {
        public List<Employee> GetEmployeWithCompany(int id);

        Company GetCompanyWithAddresses(int id);
    }
}
