using ETMS.Application.Interfaces;
using ETMS.Domain.Entities;
using ETMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ETMS.Web.Controllers
{
    [Authorize] // Must be logged in to request a transfer
    public class TransferController : Controller
    {
        private readonly ITransferRequestRepository _transferRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IDepartmentRepository _deptRepo;

        public TransferController(
            ITransferRequestRepository transferRepo,
            ILocationRepository locationRepo,
            IDepartmentRepository deptRepo)
        {
            _transferRepo = transferRepo;
            _locationRepo = locationRepo;
            _deptRepo = deptRepo;
        }

        // 1. GET: Loads the form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTransferViewModel();
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        // 2. POST: Saves the form data
        [HttpPost]
        public async Task<IActionResult> Create(CreateTransferViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If form has errors, reload the dropdowns and show the page again
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // Get the logged-in user's Employee ID from the Cookie
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim)) return RedirectToAction("Login", "Account");

            // Create the new entity
            var newRequest = new TransferRequest
            {
                EmployeeId = int.Parse(employeeIdClaim),
                ToLocationId = model.ToLocationId,
                ToDepartmentId = model.ToDepartmentId,
                TransferType = model.TransferType,
                Reason = model.Reason,
                Status = "Pending", // Default status for new requests
                RequestDate = DateTime.Now,
                IsActive = true
            };

            // Save to database
            // NOTE: Make sure your ITransferRequestRepository has an AddAsync method!
            await _transferRepo.AddAsync(newRequest);

            // Redirect back to the dashboard with a success message
            TempData["SuccessMessage"] = "Transfer Request submitted successfully!";
            return RedirectToAction("Index", "Dashboard");
        }

        // Helper method to load dropdown data from your database
        private async Task PopulateDropdownsAsync(CreateTransferViewModel model)
        {
            var locations = await _locationRepo.GetAllAsync();
            var departments = await _deptRepo.GetAllAsync();

            model.Locations = locations.Select(l => new SelectListItem
            {
                Value = l.LocationId.ToString(),
                //Text = l.Name // Change 'LocationName' to match your actual Domain Entity property
            });

            model.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName // Change 'DepartmentName' to match your actual Domain Entity property
            });
        }
    }
}