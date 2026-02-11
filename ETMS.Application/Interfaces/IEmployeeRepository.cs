<<<<<<< HEAD
﻿using System.Threading.Tasks;
using ETMS.Domain.Entities;
=======
using ETMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e

namespace ETMS.Application.Interfaces
{
    public interface IEmployeeRepository
    {
<<<<<<< HEAD
        Task<Employee?> GetEmployeeByIdAsync(int id);
    }
}
=======
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);
        Task GetByIdAsync(int currentEmployeeId);
    }
}
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
