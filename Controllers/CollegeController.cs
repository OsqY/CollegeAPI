using Microsoft.AspNetCore.Mvc;
using CollegeAPI.DTO;
using CollegeAPI.Controllers;

namespace CollegeAPI.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class CollegeController : ControllerBase
    {
        private readonly ILogger<CollegeController> _logger;

        public CollegeController(ILogger<CollegeController> logger)
        {
            _logger = logger;
        }

    }
}
