using CollegeAPI.DTO;
using CollegeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace CollegeAPI.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class CollegeController : ControllerBase
    {
        private readonly ILogger<CollegeController> _logger;
        private readonly AppDbContext _context;

        public CollegeController(ILogger<CollegeController> logger,
            AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetColleges")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<College[]>> Get([FromQuery] RequestDTO<CollegeDTO> input)
        {
            var query = _context.Colleges.AsQueryable();

            if (!string.IsNullOrEmpty(input.FilterQuery))
                query = query.Where(c => c.Name.Contains(input.FilterQuery));

            var recordCount = await query.CountAsync();
            query = query.
              OrderBy($"{input.SortColumn} {input.SortOrder}")
              .Skip(input.PageIndex * input.PageSize).Take(input.PageSize);

            return new RestDTO<College[]>()
            {
                Data = await query.ToArrayAsync(),
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = recordCount,
                Links = new List<LinkDTO>() {
                     new LinkDTO(
                         Url.Action(null, "Colleges", new {input.PageSize, input.PageIndex}, Request.Scheme)!,
                         "self","GET"
                         )
                   }
            };
        }

        [HttpPost(Name = "UpdateCollege")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<College?>> Post(CollegeDTO model)
        {
            College? college = await _context.Colleges
              .Where(c => c.Id == model.Id).FirstOrDefaultAsync();
            if (college != null)
            {
                if (!string.IsNullOrEmpty(model.Name))
                    college.Name = model.Name;
                if (!string.IsNullOrEmpty(model.Url))
                    college.Url = model.Url;
                if (!string.IsNullOrEmpty(model.Country))
                    college.Country = model.Country;

                college.LastModifiedDate = DateTime.Now;
                _context.Colleges.Update(college);
                await _context.SaveChangesAsync();
            }

            return new RestDTO<College?>()
            {
                Data = college,
                Links = new List<LinkDTO>() {
                     new LinkDTO(
                         Url.Action(null, "Colleges", model, Request.Scheme)!,
                         "self", "POST"
                         )
                   }
            }
;
        }

        [HttpDelete(Name = "DeleteCollege")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<College?>> Delete(int id)
        {
            College? college = await _context.Colleges.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (college != null)
            {
                _context.Colleges.Remove(college);
                await _context.SaveChangesAsync();
            }

            return new RestDTO<College?>()
            {
                Data = college,
                Links = new List<LinkDTO>() {
                   new LinkDTO(
                       Url.Action(null, "Colleges", id, Request.Scheme)!,
                       "self", "DELETE"
                       )
                 }
            };
        }
    }
}
