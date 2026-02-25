using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ETMS.Web.Models
{
    public class CreateTransferViewModel
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string CurrentDepartment { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string ReportingManager { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }

        [Required(ErrorMessage = "Please select a target location.")]
        public int ToLocationId { get; set; }

        [Required(ErrorMessage = "Please select a target department.")]
        public int ToDepartmentId { get; set; }

        public string NewManagerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please specify the transfer type.")]
        public string TransferType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a reason category.")]
        public string ReasonCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Effective date is required.")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveDate { get; set; }

        [Required(ErrorMessage = "Priority level is required.")]
        public string Priority { get; set; } = "Normal";

        [Required(ErrorMessage = "A detailed justification is required.")]
        [StringLength(1000, MinimumLength = 100, ErrorMessage = "Please provide a detailed reason (between 100 and 1000 characters).")]
        public string Reason { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string PersonalEmail { get; set; } = string.Empty;

        public string HandoverPlan { get; set; } = string.Empty;

        public IFormFile? SupportingDocument { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must acknowledge the disciplinary policy.")]
        public bool HasNoDisciplinaryCase { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the tenure policy.")]
        public bool MinimumTenureCompleted { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must declare the details are accurate.")]
        public bool DeclarationConfirmed { get; set; }

        public IEnumerable<SelectListItem>? Locations { get; set; }
        public IEnumerable<SelectListItem>? Departments { get; set; }
    }
}