using Microsoft.AspNetCore.Mvc;

namespace pyatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Index()
    {
        return Ok("Witaj świecie!");
    }
}
