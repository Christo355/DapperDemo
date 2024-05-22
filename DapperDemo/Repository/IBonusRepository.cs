using DapperDemo.Models;

namespace DapperDemo.Repository
{
    public interface IBonusRepository
    {
        List<Employee> GetEmployeWithCompany(int id);

        Company GetCompanyWithAddresses(int id);

        List<Company> GetAllCompanyWithEmployees();

        void AddTestCompanyWithEmployees (Company company);

        void RemoveRange(int[] companyId);

        List<Company> FilterCompanyByName(string name);

        void AddTestCompanyWithEmployeesWithTransation(Company company);

    }
}
