using System.Linq.Dynamic.Core;
using System.Text.Json;
using CollegeAPI.DTO;
using CollegeAPI.Extensions;
using CollegeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace CollegeAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CareerController : ControllerBase
    {
        private readonly ILogger<CareerController> _logger;
        private readonly AppDbContext _context;
        private readonly IDistributedCache _distributedCache;

        public CareerController(
            ILogger<CareerController> logger,
            AppDbContext context,
            IDistributedCache distributedCache
        )
        {
            _logger = logger;
            _context = context;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        [ResponseCache(CacheProfileName = "Any-60")]
        public async Task<RestDTO<Career[]>> Get([FromQuery] RequestDTO<CareerDTO> input)
        {
            (int recordCount, Career[]? result) dataTuple = (0, null);
            var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";

            if (!_distributedCache.TryGetValue(cacheKey, out dataTuple))
            {
                var query = _context.Careers.AsQueryable();
                if (!string.IsNullOrEmpty(input.FilterQuery))
                    query = query.Where(c => c.Name.Contains(input.FilterQuery));

                dataTuple.recordCount = await query.CountAsync();
                query = query
                    .OrderBy($"{input.SortColumn} {input.SortOrder}")
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize);

                dataTuple.result = await query.ToArrayAsync();
                _distributedCache.Set(cacheKey, dataTuple, new TimeSpan(0, 0, 30));
            }

            return new RestDTO<Career[]>()
            {
                Data = dataTuple.result,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = dataTuple.recordCount,
                Links = new List<LinkDTO>()
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "Careers",
                            new { input.PageIndex, input.PageSize },
                            Request.Scheme
                        )!,
                        "self",
                        "GET"
                    )
                }
            };
        }

        [HttpPost(Name = "UpdateCareer")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<RestDTO<Career?>> Post(CareerDTO model)
        {
            Career? career = await _context
                .Careers.Where(c => c.Id == model.Id)
                .FirstOrDefaultAsync();

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
                Links = new List<LinkDTO>()
                {
                    new LinkDTO(Url.Action(null, "Careers", model, Request.Scheme)!, "self", "POST")
                }
            };
        }

        [HttpDelete(Name = "DeleteCareer")]
        [ResponseCache(CacheProfileName = "NoCache")]
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
                Links = new List<LinkDTO>()
                {
                    new LinkDTO(Url.Action(null, "Careers", id, Request.Scheme)!, "self", "POST")
                }
            };
        }
    }
}
