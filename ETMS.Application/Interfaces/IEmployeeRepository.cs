using ETMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETMS.Application.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);
        Task GetByIdAsync(int currentEmployeeId);
    }
}