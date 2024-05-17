using CsvHelper.Configuration.Attributes;

namespace CollegeAPI.Models.Csv
{
    public class CollegeRecord
    {
        [Name("CollegeUrl")]
        public string? CollegeUrl { get; set; }

        [Name("CareerUrl")]
        public string? CareerUrl { get; set; }

        [Name("Title")]
        public string? Title { get; set; }

        [Name("CollegeName")]
        public string? CollegeName { get; set; }

        [Name("Contact")]
        public string? Contact { get; set; }

        [Name("Country")]
        public string? Country { get; set; }
    }
}
