using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETMS.Domain.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }
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

        public Department? Department { get; set; }
        public Location? Location { get; set; }
        public Employee? ReportingManager { get; set; }

        public string? Grade { get; set; }
        public string? SBU { get; set; }
        public string? CostCenter { get; set; }
        public string? Company { get; set; }
        public string? HRBP { get; set; }
    }
}