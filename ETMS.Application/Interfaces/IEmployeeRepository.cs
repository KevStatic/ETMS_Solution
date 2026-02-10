using System.Threading.Tasks;
using ETMS.Domain.Entities;

namespace ETMS.Application.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetEmployeeByIdAsync(int id);
    }
}
