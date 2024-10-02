using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeterShared.Models;
using MeterDataLayer;
using Microsoft.Data.Sqlite;
using System.Globalization;
using Dapper;

namespace MeterApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeterReadingController : Controller
    {
        private readonly string _connectionString;
        public MeterReadingController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("meter-reading-uploads")]
        public async Task<IActionResult> UploadMeterReadings(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not provided.");

            var successfulReadings = 0;
            var failedReadings = 0;

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var csvLines = reader.ReadToEnd().Split(Environment.NewLine);
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open(); 
                    
                    foreach (var line in csvLines.Skip(1))// Skip the header
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        var fields = line.Split(',');
                        if (fields.Length != 3)
                        {
                            failedReadings++;
                            continue;
                        }
                        if (!int.TryParse(fields[0], out var accountId)
                            || !int.TryParse(fields[1], out var meterReadingValue)
                            || !System.Text.RegularExpressions.Regex.IsMatch(fields[1], @"^\d{5}$")
                            || !DateTime.TryParseExact(fields[2], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var readingDate))
                        {
                            failedReadings++;
                            continue;
                        }

                        // Check if account exists
                        var account = connection.QueryFirstOrDefault(
                            "SELECT * FROM Accounts WHERE AccountId = @AccountId",
                            new
                            {
                                AccountId = accountId
                            });

                        if (account == null)
                        {
                            failedReadings++;
                            continue;
                        }
                        // Check if meter reading already exists for this account and value
                        var existingReading = connection.QueryFirstOrDefault(
                            "SELECT * FROM MeterReadings WHERE AccountId = @AccountId " +
                            "AND MeterReadingValue = @MeterReadingValue",
                            new
                            {
                                AccountId = accountId,
                                MeterReadingValue = meterReadingValue
                            });

                        if (existingReading != null)
                        {
                            failedReadings++;
                            continue;
                        }

                        // Insert valid reading
                        var insertQuery = @" INSERT INTO MeterReadings (AccountId, MeterReadingValue, ReadingDate) 
                    VALUES (@AccountId, @MeterReadingValue, @ReadingDate)";

                        connection.Execute(
                            insertQuery,
                            new
                            {
                                AccountId = accountId,
                                MeterReadingValue = meterReadingValue,
                                ReadingDate = readingDate.ToString("yyyy-MM-dd")
                            });
                        successfulReadings++;
                    }
                }
            }
            return Ok(
                new
                {
                    successfulReadings,
                    failedReadings
                });
        }
    }
}
