using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class Education
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        [Required]
        public string Degree { get; set; }

        [Required]
        public int PassingYear { get; set; }

        [Range(0, 100)]
        public decimal Percentage { get; set; }
    }

}
