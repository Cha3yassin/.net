using System.ComponentModel.DataAnnotations;

namespace WebApplicationEmploye.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        public string Departement { get; set; }

        [Range(500, 10000)]
        public int Salary { get; set; }
    }
}
