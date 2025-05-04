using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using EmployeeManagement.Helper;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string EmployeeCode { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        public int Age => DateTime.Now.Year - Dob.Year;

        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        public string ContactNo { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]

        public string Email { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public string? PhotoPath { get; set; }

        [NotMapped]
        [Display(Name = "Upload Photo")]
        [DataType(DataType.Upload)]
        [FileValidation(new string[] { ".jpg", ".jpeg", ".png" }, 2)]
        public IFormFile PhotoFile { get; set; }

        public ICollection<Education> Educations { get; set; }
    }
}
