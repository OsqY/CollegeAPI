using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeAPI.Models
{
    [Table("Colleges")]
    public class College
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Url { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime LastModifiedDate { get; set; }

        public ICollection<College_Careers> College_Careers { get; set; }
    }
}
