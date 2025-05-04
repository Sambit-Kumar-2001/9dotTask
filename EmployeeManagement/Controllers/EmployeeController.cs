using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Models;
using EmployeeManagement.Interfaces;

namespace EmployeeManagement.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepo;

        public EmployeeController(IEmployeeRepository employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _employeeRepo.GetAllAsync();
            return View(employees);
        }

        public IActionResult Create()
        {
            return View(new Employee());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee, IFormFile Photo)
        {
            if (Photo != null && Photo.Length > 0)
            {
                var extension = Path.GetExtension(Photo.FileName).ToLower();

                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    ModelState.AddModelError("Photo", "Only .jpg, .jpeg, and .png files are allowed.");
                    return View(employee); 
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Photo.CopyToAsync(stream);
                }

                employee.PhotoPath = "/uploads/" + fileName;  
            }
            else
            {
                employee.PhotoPath = null;
            }

            var exists = await _employeeRepo.EmployeeExistsAsync(employee.ContactNo, employee.Dob);
            if (exists)
            {
                ModelState.AddModelError("", "Employee with same contact number and DOB already exists.");
                return View(employee);  
            }

            var lastEmployee = await _employeeRepo.GetLastEmployeeAsync();
            int nextId = (lastEmployee?.Id ?? 0) + 1;
            employee.EmployeeCode = $"Emp{nextId:D3}";

             await _employeeRepo.AddAsync(employee);

             return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeRepo.GetByIdWithEducationAsync(id);
            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee, IFormFile? Photo)
        {
            if (id != employee.Id)
                return BadRequest();

           

            var existingEmployee = await _employeeRepo.GetByIdWithEducationAsync(id);
            if (existingEmployee == null)
                return NotFound();

             var duplicateExists = await _employeeRepo.EmployeeExistsAsync(employee.ContactNo, employee.Dob, id);
            if (duplicateExists)
            {
                ModelState.AddModelError("", "Another employee with the same contact number and DOB already exists.");
                return View(employee);
            }

             if (Photo != null && Photo.Length > 0)
            {
                var extension = Path.GetExtension(Photo.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    ModelState.AddModelError("Photo", "Only .jpg, .jpeg, and .png files are allowed.");
                    return View(employee);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Photo.CopyToAsync(stream);
                }

                 if (!string.IsNullOrEmpty(existingEmployee.PhotoPath))
                {
                    var oldFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingEmployee.PhotoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                }

                existingEmployee.PhotoPath = "/uploads/" + uniqueFileName;
            }

             existingEmployee.FirstName = employee.FirstName;
            existingEmployee.MiddleName = employee.MiddleName;
            existingEmployee.LastName = employee.LastName;
            existingEmployee.Dob = employee.Dob;
            existingEmployee.Gender = employee.Gender;
            existingEmployee.ContactNo = employee.ContactNo;
            existingEmployee.Email = employee.Email;

             await _employeeRepo.ReplaceEducationsAsync(existingEmployee, employee.Educations.ToList());

             await _employeeRepo.UpdateAsync(existingEmployee);

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _employeeRepo.GetByIdAsync(id); 
            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string ConfirmationText)
        {
            var employee = await _employeeRepo.GetByIdAsync(id);

            if (employee == null)
                return NotFound();

            var expectedText = $"DELETE {employee.EmployeeCode}";

            if (!string.Equals(ConfirmationText?.Trim(), expectedText, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", $"Incorrect confirmation text. Please type '{expectedText}' to delete.");
                return View("Delete", employee);
            }

            await _employeeRepo.DeleteAsync(id);  
            return RedirectToAction(nameof(Index));
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _employeeRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null)
                return NotFound();

            return View(employee);
        }


        public async Task<IActionResult> Report(DateTime? fromDate, DateTime? toDate, string searchKeyword = "", int page = 1, int pageSize = 10)
        {
             if (fromDate.HasValue)
                fromDate = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);

            if (toDate.HasValue)
                toDate = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                ModelState.AddModelError("", "The 'From Date' cannot be later than the 'To Date'.");
                return View();
            }

            var employees = await _employeeRepo.GetEmployeesReportAsync(fromDate, toDate, searchKeyword, page, pageSize);
            var totalRecords = await _employeeRepo.GetEmployeesCountAsync(fromDate, toDate, searchKeyword);
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
            ViewData["SearchKeyword"] = searchKeyword;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View("Report", employees);
        }
    }
}
