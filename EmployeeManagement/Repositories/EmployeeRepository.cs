using EmployeeManagement.Interfaces;
using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeDbContext _context;

        public EmployeeRepository(EmployeeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees.Include(e => e.Educations).ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Educations) // Eager loading Educations
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Employee employee)
        {
            // Normalize DOB to UTC
            if (employee.Dob.Kind == DateTimeKind.Unspecified)
            {
                employee.Dob = DateTime.SpecifyKind(employee.Dob, DateTimeKind.Utc);
            }
            else if (employee.Dob.Kind == DateTimeKind.Local)
            {
                employee.Dob = employee.Dob.ToUniversalTime();
            }

            if (employee.PhotoFile != null && employee.PhotoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(employee.PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await employee.PhotoFile.CopyToAsync(stream);
                }

                // Save relative path to DB
                employee.PhotoPath = "/uploads/" + fileName;
            }


            // Reset education IDs and assign EmployeeId
            if (employee.Educations != null && employee.Educations.Any())
            {
                foreach (var education in employee.Educations)
                {
                    education.Id = 0; // ensure EF treats it as a new record
                }
            }

            // Add employee (EF will add Educations too via navigation property)
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            // Normalize DOB to UTC before updating
            if (employee.Dob.Kind == DateTimeKind.Unspecified)
            {
                employee.Dob = DateTime.SpecifyKind(employee.Dob, DateTimeKind.Utc);
            }
            else if (employee.Dob.Kind == DateTimeKind.Local)
            {
                employee.Dob = employee.Dob.ToUniversalTime();
            }

            // Handle the employee photo upload/update if a new photo is provided
            if (employee.PhotoFile != null && employee.PhotoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate a new file name and save the photo
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(employee.PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await employee.PhotoFile.CopyToAsync(stream);
                }

                // Save relative file path in the database
                employee.PhotoPath = "/uploads/" + fileName;
            }

            // Attach the employee entity for update
            _context.Employees.Update(employee);

            // Update the related education records
            if (employee.Educations != null && employee.Educations.Any())
            {
                foreach (var education in employee.Educations)
                {
                    education.EmployeeId = employee.Id; // Ensure foreign key is correctly set
                }

                // Update or add education records (assuming the education list is properly tracked in the model)
                _context.Educations.UpdateRange(employee.Educations);
            }

            // Commit changes to the database
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var emp = await _context.Employees.Include(e => e.Educations).FirstOrDefaultAsync(e => e.Id == id);
            if (emp != null)
            {
                // Delete related education records
                _context.Educations.RemoveRange(emp.Educations);

                // Delete the employee
                _context.Employees.Remove(emp);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> EmployeeExistsAsync(string contactNo, DateTime dob)
        {
            // Ensure the DateTime.Kind is UTC to avoid InvalidCastException
            if (dob.Kind == DateTimeKind.Unspecified)
            {
                dob = DateTime.SpecifyKind(dob, DateTimeKind.Utc);
            }
            else if (dob.Kind == DateTimeKind.Local)
            {
                dob = dob.ToUniversalTime();
            }

            return await _context.Employees.AnyAsync(e => e.ContactNo == contactNo && e.Dob == dob);
        }


        public async Task<Employee?> GetLastEmployeeAsync()
        {
            return await _context.Employees
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync();
        }
        public async Task<Employee?> GetByIdWithEducationAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Educations)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task ReplaceEducationsAsync(Employee existingEmployee, List<Education> newEducations)
        {
            var existingEducations = await _context.Educations
                .Where(e => e.EmployeeId == existingEmployee.Id)
                .ToListAsync();

            _context.Educations.RemoveRange(existingEducations); // delete all
            foreach (var edu in newEducations)
            {
                edu.Id = 0; // Ensure EF treats them as new
                edu.EmployeeId = existingEmployee.Id;
            }

            await _context.Educations.AddRangeAsync(newEducations);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmployeeExistsAsync(string contactNo, DateTime dob, int? excludeId = null)
        {
            if (dob.Kind == DateTimeKind.Unspecified)
                dob = DateTime.SpecifyKind(dob, DateTimeKind.Utc);
            else if (dob.Kind == DateTimeKind.Local)
                dob = dob.ToUniversalTime();

            return await _context.Employees
                .AnyAsync(e => e.ContactNo == contactNo && e.Dob == dob && (!excludeId.HasValue || e.Id != excludeId));
        }
        public async Task<IEnumerable<Employee>> GetEmployeesReportAsync(DateTime? fromDate, DateTime? toDate, string searchKeyword, int page, int pageSize)
        {
            IQueryable<Employee> query = _context.Employees.Include(e => e.Educations);

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.Dob >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.Dob <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(e => e.FirstName.Contains(searchKeyword) || e.LastName.Contains(searchKeyword) || e.ContactNo.Contains(searchKeyword));
            }

            // Apply pagination
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return await query.ToListAsync();
        }

        public async Task<int> GetEmployeesCountAsync(DateTime? fromDate, DateTime? toDate, string searchKeyword)
        {
            IQueryable<Employee> query = _context.Employees;

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.Dob >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.Dob <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(e => e.FirstName.Contains(searchKeyword) || e.LastName.Contains(searchKeyword) || e.ContactNo.Contains(searchKeyword));
            }

            return await query.CountAsync();
        }

    }
}
