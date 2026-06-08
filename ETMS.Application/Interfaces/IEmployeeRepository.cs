using ETMS.Application.DTOs.Profile;
using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);

        // ? FIXED: was Task (void), now Task<Employee?>
        Task<Employee?> GetByIdAsync(int currentEmployeeId);

        // Profile methods
        Task<EmployeeProfileDto?> GetProfileByEmployeeIdAsync(int employeeId);
        Task<bool> UpdateProfileAsync(int employeeId, UpdateProfileDto dto);
    }
}
