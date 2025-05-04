using EmployeeManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeManagement.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(int id);
        Task<bool> EmployeeExistsAsync(string contactNo, DateTime dob);
        Task<Employee?> GetLastEmployeeAsync();
        Task<Employee?> GetByIdWithEducationAsync(int id);
        Task ReplaceEducationsAsync(Employee existingEmployee, List<Education> newEducations);
        Task<bool> EmployeeExistsAsync(string contactNo, DateTime dob, int? excludeId = null);
        Task<IEnumerable<Employee>> GetEmployeesReportAsync(DateTime? fromDate, DateTime? toDate, string searchKeyword, int page, int pageSize);
        Task<int> GetEmployeesCountAsync(DateTime? fromDate, DateTime? toDate, string searchKeyword);

    }
}
