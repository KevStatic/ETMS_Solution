using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ETMS.Web.Models
{
    public class CreateTransferViewModel
    {
        [Required(ErrorMessage = "Please select a target location.")]
        [Display(Name = "Target Location")]
        public int ToLocationId { get; set; }

        [Required(ErrorMessage = "Please select a target department.")]
        [Display(Name = "Target Department")]
        public int ToDepartmentId { get; set; }

        [Required(ErrorMessage = "Please select the transfer type.")]
        [Display(Name = "Transfer Type")]
        public string TransferType { get; set; } // e.g., "Permanent", "Temporary"

        [Required(ErrorMessage = "Please provide a reason for the transfer.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; }

        // These are used to populate the dropdown menus in the UI
        public IEnumerable<SelectListItem>? Locations { get; set; }
        public IEnumerable<SelectListItem>? Departments { get; set; }
    }
}