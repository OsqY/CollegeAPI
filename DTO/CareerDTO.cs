using System.ComponentModel.DataAnnotations;

namespace CollegeAPI.DTO
{
    public class CareerDTO
    {
        [Required]
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}
