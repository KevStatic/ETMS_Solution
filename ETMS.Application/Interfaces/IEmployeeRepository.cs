using ETMS.Application.DTOs.Profile;
using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);
        Task GetByIdAsync(int currentEmployeeId);

        // Profile methods
        Task<EmployeeProfileDto?> GetProfileByUserAccountIdAsync(int userAccountId);
        Task<bool> UpdateProfileAsync(int userAccountId, UpdateProfileDto dto);
    }
}