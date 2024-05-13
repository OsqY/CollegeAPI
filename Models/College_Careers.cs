using System.ComponentModel.DataAnnotations;

namespace CollegeAPI.Models
{
    public class College_Careers
    {
        [Key]
        public int CollegeId { get; set; }

        [Key]
        public int CareerId { get; set; }

        public DateTime CreatedDate { get; set; }

        public Career? Career { get; set; }
        public College? College { get; set; }
    }
}
