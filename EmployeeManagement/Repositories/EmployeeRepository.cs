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
                .Include(e => e.Educations)  
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

                
                employee.PhotoPath = "/uploads/" + fileName;
            }


             if (employee.Educations != null && employee.Educations.Any())
            {
                foreach (var education in employee.Educations)
                {
                    education.Id = 0;  
                }
            }

             await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
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

                 employee.PhotoPath = "/uploads/" + fileName;
            }

             _context.Employees.Update(employee);

             if (employee.Educations != null && employee.Educations.Any())
            {
                foreach (var education in employee.Educations)
                {
                    education.EmployeeId = employee.Id; 
                }

                 _context.Educations.UpdateRange(employee.Educations);
            }

             await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var emp = await _context.Employees.Include(e => e.Educations).FirstOrDefaultAsync(e => e.Id == id);
            if (emp != null)
            {
                 _context.Educations.RemoveRange(emp.Educations);

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

            _context.Educations.RemoveRange(existingEducations); 
            foreach (var edu in newEducations)
            {
                edu.Id = 0;
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
                query = query.Where(e => e.Dob >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.Dob <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                query = query.Where(e =>
                    EF.Functions.ILike(e.FirstName, $"%{searchKeyword}%") ||
                    EF.Functions.ILike(e.LastName, $"%{searchKeyword}%") ||
                    EF.Functions.ILike(e.ContactNo, $"%{searchKeyword}%") ||
                    EF.Functions.ILike(e.EmployeeCode, $"%{searchKeyword}%")
                );
            }

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
