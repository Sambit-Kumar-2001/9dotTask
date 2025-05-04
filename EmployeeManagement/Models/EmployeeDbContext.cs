using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EmployeeManagement.Helper;

namespace EmployeeManagement.Models
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
            : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Education> Educations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var genderConverter = new EnumToStringConverter<Gender>();

            modelBuilder.Entity<Employee>()
                .Property(e => e.Gender)
                .HasConversion(genderConverter);
        }
    }
}
