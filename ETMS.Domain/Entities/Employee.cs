<<<<<<< HEAD
﻿namespace ETMS.Domain.Entities
=======
﻿using System;

namespace ETMS.Domain.Entities
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
{
    public class Employee
    {
        public int EmployeeId { get; set; }
<<<<<<< HEAD
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
=======
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfJoining { get; set; }
        public string EmploymentType { get; set; }

        // Foreign Keys
        public int DepartmentId { get; set; }
        public int BranchId { get; set; }
        public int LocationId { get; set; }
        public int DesignationId { get; set; }
        public int? ReportingManagerId { get; set; }

        public string Status { get; set; }
        public bool IsActive { get; set; }

        // Extra properties for display 
    }
}
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
