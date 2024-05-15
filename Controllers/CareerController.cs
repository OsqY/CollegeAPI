using CollegeAPI.DTO;
using CollegeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace CollegeAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CareerController : ControllerBase
    {
        private readonly ILogger<CareerController> _logger;
        private readonly AppDbContext _context;

        public CareerController(ILogger<CareerController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<Career[]>> Get([FromQuery] RequestDTO<CareerDTO> input)
        {
            var query = _context.Careers.AsQueryable();

            if (!string.IsNullOrEmpty(input.FilterQuery))
                query = query.Where(c => c.Name.Contains(input.FilterQuery));

            query = query.OrderBy($"{input.SortColumn} {input.SortOrder}")
              .Skip(input.PageIndex * input.PageSize).Take(input.PageSize);

            return new RestDTO<Career[]>()
            {
                Data = await query.ToArrayAsync(),
                Links = new List<LinkDTO>() {
                     new LinkDTO(
                         Url.Action(null, "Careers", new {input.PageIndex, input.PageSize}, Request.Scheme)!,
                         "self","GET"
                         )
                   }

            };
        }


        [HttpPost(Name = "UpdateCareer")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<Career?>> Post(CareerDTO model)
        {
            Career? career = await _context.Careers.Where(c => c.Id == model.Id).FirstOrDefaultAsync();

            if (career != null)
            {
                if (model.Name != null)
                    career.Name = model.Name;
                career.LastModifiedDate = DateTime.Now;
                _context.Careers.Update(career);
                await _context.SaveChangesAsync();
            }

            return new RestDTO<Career?>()
            {
                Data = career,
                Links = new List<LinkDTO>() {
                     new LinkDTO(
                         Url.Action(null, "Careers", model, Request.Scheme)!,
                         "self", "POST"
                         )
                   }
            };
        }

        [HttpDelete(Name = "DeleteCareer")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<Career?>> Delete(int id)
        {
            Career? career = await _context.Careers.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (career != null)
            {
                _context.Careers.Remove(career);
                await _context.SaveChangesAsync();
            }
            return new RestDTO<Career?>()
            {
                Data = career,
                Links = new List<LinkDTO>() {
                     new LinkDTO(
                         Url.Action(null, "Careers", id, Request.Scheme)!,
                         "self", "POST"
                         )
                   }
            };
        }

    }
}
