using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name should only contain letters and spaces.")]
        public string FirstName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name should only contain letters and spaces.")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; } // Date of Birth

        public int Age => DateTime.Now.Year - Dob.Year;

        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        public string ContactNo { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Gender { get; set; } // "Male", "Female"

        // Navigation Property
        public ICollection<Education> Educations { get; set; }
    }
}
