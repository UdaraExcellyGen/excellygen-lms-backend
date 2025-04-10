using Microsoft.AspNetCore.Mvc;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloTestController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Ping successful! This is a test endpoint to verify GitHub commits try again.");
        }
    }
}
