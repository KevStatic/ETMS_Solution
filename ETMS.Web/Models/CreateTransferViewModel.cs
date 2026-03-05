using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETMS.Web.Models
{
    public class CreateTransferViewModel
    {

        // --- LEFT PANEL: CURRENT INFO ---
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string SBU { get; set; } = string.Empty;
        public string CurrentDepartment { get; set; } = string.Empty;
        public string CostCenter { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public string CurrentCountry { get; set; } = string.Empty;
        public string Company { get; set; } = "Larsen & Toubro Limited";
        public string ImmediateSupervisor { get; set; } = string.Empty;
        public string DepartmentHead { get; set; } = string.Empty;
        public string HRBP { get; set; } = string.Empty;

        // Dynamic personnel details
        public string ISName { get; set; } = string.Empty;
        public string ISCode { get; set; } = string.Empty;
        public string DHName { get; set; } = string.Empty;
        public string DHCode { get; set; } = string.Empty;
        public string HRBPName { get; set; } = string.Empty;

        // --- RIGHT PANEL: FORM FIELDS ---

        // Transfer Information (From/To)
        public string FromType { get; set; } = string.Empty;
        public int FromLocationId { get; set; }

        [Required(ErrorMessage = "Please select target type")]
        public string ToType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select target location")]
        public int ToLocationId { get; set; }

        // Logistics
        [Required]
        public string WithinCity { get; set; } = "No"; // Yes or No
        [Required]
        public string RelocationStatus { get; set; } = "Bachelor"; // Bachelor or Family
        public string TransferTypeAuto { get; set; } = "Permanent"; // Usually read-only/auto-selected

        // NEW: LetterType was missing and is used by TransferController
        public string LetterType { get; set; } = string.Empty;

        // Details
        [Required]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Required]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        public string NewVertical { get; set; } = string.Empty;
        [Required]
        public int ToDepartmentId { get; set; }
        [Required]
        public string NewBU { get; set; } = string.Empty;

        public string NewISPsno { get; set; } = string.Empty;
        public string NewISName { get; set; } = string.Empty;
        public string NewISEmail { get; set; } = string.Empty;

        [Required]
        public string ICHead { get; set; } = string.Empty;

        [Required]
        public string Remarks { get; set; } = string.Empty;

        // Dropdowns
        public IEnumerable<SelectListItem>? Locations { get; set; }
        public IEnumerable<SelectListItem>? Departments { get; set; }
        public IEnumerable<ETMS.Domain.Entities.Location>? RawLocations { get; set; }

        // Transfer Type
        public string TransferType { get; set; } = "Permanent"; // Usually read-only/auto-selected
    }
}