using CollegeAPI.Models;
using CollegeAPI.Models.Csv;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CollegeAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SeedingController : ControllerBase
    {

        readonly ILogger<SeedingController> _logger;
        readonly AppDbContext _context;
        readonly IWebHostEnvironment _env;

        public SeedingController(ILogger<SeedingController> logger,
            AppDbContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _env = env;
        }

        [HttpPost(Name = "Seeding")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Post()
        {
            var config = new CsvConfiguration(CultureInfo.GetCultureInfo("pt-BR"))
            {
                HasHeaderRecord = true,
                Delimiter = ","
            };

            using var reader = new StreamReader(System.IO.Path.Combine(_env.ContentRootPath, "Data/Colleges.csv"));
            using var csv = new CsvReader(reader, config);

            var existingColleges = await _context.Colleges.ToDictionaryAsync(c => c.Name);

            var existingCareers = await _context.Careers.ToDictionaryAsync(c => c.Name);

            var now = DateTime.Now;

            var records = csv.GetRecords<CollegeRecord>();

            int i = existingColleges.Count();
            foreach (var record in records)
            {
                bool isDifferentCollegeFromDictionary = false;
                var college = existingColleges.GetValueOrDefault(record.CollegeName);
                if (college == null)
                {
                    college = new College()
                    {
                        Name = record.CollegeName!,
                        Country = record.Country!,
                        Url = record.CollegeUrl!,
                        CreatedDate = now,
                        LastModifiedDate = now

                    };
                    i++;
                    isDifferentCollegeFromDictionary = true;
                    existingColleges.Add(college.Name, college);
                    _context.Colleges.Add(college);
                }

                var career = existingCareers.GetValueOrDefault(record.Title);
                if (career == null)
                {
                    career = new Career()
                    {
                        Name = record.Title!,
                        Url = record.CareerUrl ?? record.CollegeUrl,
                        CreatedDate = now,
                        LastModifiedDate = now,
                    };

                    if (isDifferentCollegeFromDictionary)
                    {
                        career.CollegeId = college.Id;
                        career.College = college;
                    }
                    else
                    {
                        var colleges = existingColleges.ToList();
                        var lastCollege = colleges[i - 1];
                        career.CollegeId = lastCollege.Value.Id;
                        career.College = lastCollege.Value;
                    }

                    existingCareers.Add(career.Name, career);
                    _context.Careers.Add(career);
                }


            }
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                Colleges = _context.Colleges.Count(),
                Careers = _context.Careers.Count()

            });
        }

    }
}
