using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeterShared.Models;
using MeterDataLayer;

namespace MeterApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeterReadingController : Controller
    {
        private readonly MeterReadingDataBase _databaseContext;
        public MeterReadingController(MeterReadingDataBase databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet]
        [Route("meter-reading-uploads")]
        public async Task<IActionResult> DefaultEndpoint()
        {
            return Ok("Working");
        }

        [HttpPost]
        [Route("meter-reading-uploads")]
        public async Task<IActionResult> UploadMeterReadings(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file to upload.");

            var meterReadings = new List<MeterReadingDto>();

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var data = line.Split(',');

                    var meterReading = new MeterReadingDto()
                    {
                        AccountId = int.Parse(data[0]),
                        MeterReadingDate = DateTime.Parse(data[1]),
                        MaterReadingValue = int.Parse(data[2])
                    };

                    meterReadings.Add(meterReading);
                }
            }

            //Validate
            foreach (var reading in meterReadings)
            {
                var accountExists = await _databaseContext.Accounts
                    .AnyAsync(a => a.AccountId == reading.AccountId);

                if (!accountExists)
                    continue;

                var existingReading = await _databaseContext.MeterReadings
                    .AnyAsync(m => m.AccountId == reading.AccountId
                    && m.MeterReadingDate == reading.MeterReadingDate);

                if (existingReading)
                    continue;

                var meterReading = new MeterReading()
                {
                    AccountId = reading.AccountId,
                    MeterReadingDate = reading.MeterReadingDate,
                    MeterReadingValue = reading.MaterReadingValue
                };

                await _databaseContext.MeterReadings.AddAsync(meterReading);
            }

            await _databaseContext.SaveChangesAsync();

            return Ok("Meter readings have been processed.");
        }
    }
}
