using System.Linq.Dynamic.Core;
using System.Text.Json;
using CollegeAPI.Attributes;
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
    public class CollegeController : ControllerBase
    {
        private readonly ILogger<CollegeController> _logger;
        private readonly AppDbContext _context;
        private readonly IDistributedCache _distributedCache;

        public CollegeController(
            ILogger<CollegeController> logger,
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
        [ManualValidationFilter]
        public async Task<ActionResult<RestDTO<College[]>>> Get(
            [FromQuery] RequestDTO<CollegeDTO> input
        )
        {
            if (!ModelState.IsValid)
            {
                var details = new ValidationProblemDetails(ModelState);
                details.Extensions["traceId"] =
                    System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;

                if (ModelState.Keys.Any(k => k == "PageSize"))
                {
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.2";
                    details.Status = StatusCodes.Status501NotImplemented;
                    return new ObjectResult(details)
                    {
                        StatusCode = StatusCodes.Status501NotImplemented
                    };
                }
                else
                {
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new ObjectResult(details);
                    ;
                }
            }

            (int recordCount, College[]? result) dataTuple = (0, null);
            var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";

            if (_distributedCache.TryGetValue(cacheKey, out dataTuple))
            {
                var query = _context.Colleges.AsQueryable();

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

            return new RestDTO<College[]>()
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
                            "Colleges",
                            new { input.PageSize, input.PageIndex },
                            Request.Scheme
                        )!,
                        "self",
                        "GET"
                    )
                }
            };
        }

        [HttpPost(Name = "UpdateCollege")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<RestDTO<College?>> Post(CollegeDTO model)
        {
            College? college = await _context
                .Colleges.Where(c => c.Id == model.Id)
                .FirstOrDefaultAsync();
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
                Links = new List<LinkDTO>()
                {
                    new LinkDTO(
                        Url.Action(null, "Colleges", model, Request.Scheme)!,
                        "self",
                        "POST"
                    )
                }
            };
        }

        [HttpDelete(Name = "DeleteCollege")]
        [ResponseCache(CacheProfileName = "NoCache")]
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
                Links = new List<LinkDTO>()
                {
                    new LinkDTO(Url.Action(null, "Colleges", id, Request.Scheme)!, "self", "DELETE")
                }
            };
        }
    }
}
