using Microsoft.AspNetCore.Mvc;

namespace ExcellyGenLMS.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthTestController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Auth test successful");
        }
    }
}
