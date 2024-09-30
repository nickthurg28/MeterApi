using Microsoft.AspNetCore.Mvc;

namespace MeterApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeterReading : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("meter-reading-uploads")]
        public async Task<IActionResult> DefaultEndpoint()
        {
            return Ok("Working");
        }
        
        [HttpPost]
        [Route("meter-reading-uploads")]
        public async Task<IActionResult> UploadMeterReadings()
        {
            return Ok("Working");
        }
    }
}
