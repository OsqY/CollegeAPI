using System.ComponentModel.DataAnnotations;

namespace CollegeAPI.DTO
{
    public class CareerDTO
    {
        [Required]
        public int Id { get; set; }

        [RegularExpression(@"^[A-Za-z\s]+$")]
        public string? Name { get; set; }
    }
}
