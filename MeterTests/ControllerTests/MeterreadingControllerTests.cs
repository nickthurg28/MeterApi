using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using MeterApi.Controllers;
using MeterShared.Models;
using Microsoft.Data.Sqlite;
using MeterApi.Models;

namespace MeterApi.Tests
{
    public class MeterReadingControllerTests
    {
        private MeterReadingController _controller;
        private readonly IConfiguration _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", "DataSource=:memory:;Mode=Memory;Cache=Shared" }
            }).Build();

        public MeterReadingControllerTests()
        {
            _controller = new MeterReadingController(_configuration);
        }

        private Mock<IFormFile> CreateMockFile(string content, string fileName = "test.csv")
        {
            var mockFile = new Mock<IFormFile>();
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);
            return mockFile;
        }

        [Fact]
        public async Task UploadMeterReadings_ReturnsBadRequest_WhenFileIsNull()
        {
            // Act
            var result = await _controller.UploadMeterReadings(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("File not provided.", badRequestResult.Value);
        }

        [Fact]
        public async Task UploadMeterReadings_ReturnsBadRequest_WhenFileIsEmpty()
        {
            // Arrange
            var mockFile = CreateMockFile(string.Empty);

            // Act
            var result = await _controller.UploadMeterReadings(mockFile.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("File not provided.", badRequestResult.Value);
        }

        [Fact]
        public async Task UploadMeterReadings_ReturnsOk_WhenFileHasValidReadings()
        {
            // Arrange
            var csvContent = "AccountId,ReadingDate,MeterReadingValue\r\n1234,24/04/2019 09:24,12345";
            var mockFile = CreateMockFile(csvContent);

            // Create a connection string that ensures the in-memory database stays alive across connections
            var connectionString = "DataSource=:memory:;Mode=Memory;Cache=Shared";

            // Create a single SQLite connection for the entire test and share across connections
            await using var connection = new SqliteConnection(connectionString);
            connection.Open(); // Keep connection open throughout the test

            // Create tables
            await connection.ExecuteAsync("CREATE TABLE Accounts (AccountId INT, FirstName TEXT, LastName TEXT)");
            await connection.ExecuteAsync("CREATE TABLE MeterReadings (AccountId INT, MeterReadingValue INT, ReadingDate TEXT)");

            // Insert test data into the Accounts table
            await connection.ExecuteAsync("INSERT INTO Accounts (AccountId, FirstName, LastName) VALUES (1234, 'John', 'Doe')");

            // Mock configuration to return the in-memory connection string
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["ConnectionStrings:DefaultConnection"]).Returns(connectionString);

            // Initialize controller with the mock configuration
            var controller = new MeterReadingController(_configuration);

            // Act
            var result = await controller.UploadMeterReadings(mockFile.Object) as OkObjectResult;

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StatusCode.Should().Be((int)HttpStatusCode.OK);

                var resultObj = result.Value as UploadResult;
                resultObj.Should().NotBeNull();
                resultObj.SuccessfulReadings.Should().Be(1);
                resultObj.FailedReadings.Should().Be(0);
            }
        }

        [Fact]
        public async Task UploadMeterReadings_ReturnsOk_WhenFileHasInvalidReadings()
        {
            // Arrange
            var csvContent = "AccountId,ReadingDate,MeterReadingValue\r\n1234,invalid_date,abc";
            var mockFile = CreateMockFile(csvContent);

            // Seed test database with AccountId 1234
            await using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            await connection.ExecuteAsync("CREATE TABLE Accounts (AccountId INT, FirstName TEXT, LastName TEXT)");
            await connection.ExecuteAsync("CREATE TABLE MeterReadings (AccountId INT, MeterReadingValue INT, ReadingDate TEXT)");
            await connection.ExecuteAsync("INSERT INTO Accounts (AccountId, FirstName, LastName) VALUES (1234, 'John', 'Doe')");

            _controller = new MeterReadingController(_configuration);

            // Act
            var result = await _controller.UploadMeterReadings(mockFile.Object) as OkObjectResult;

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StatusCode.Should().Be((int)HttpStatusCode.OK);

                var  resultObj = result.Value as UploadResult;
                resultObj.SuccessfulReadings.Should().Be(0);
                resultObj.FailedReadings.Should().Be(1);
            }
        }

        [Fact]
        public async Task UploadMeterReadings_ReturnsOk_WhenFileHasDuplicateReadings()
        {
            // Arrange
            var csvContent = "AccountId,ReadingDate,MeterReadingValue\r\n1234,24/04/2019 09:24,12345\r\n1234,24/04/2019 09:24,12345";
            var mockFile = CreateMockFile(csvContent);

            // Use a shared cache to keep the in-memory database alive across connections
            var connectionString = "DataSource=:memory:;Mode=Memory;Cache=Shared";

            // Create a single SQLite connection for the entire test and share it
            await using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Create tables 
            await connection.ExecuteAsync("CREATE TABLE Accounts (AccountId INT, FirstName TEXT, LastName TEXT)");
            await connection.ExecuteAsync("CREATE TABLE MeterReadings (AccountId INT, MeterReadingValue INT, ReadingDate TEXT)");

            // Seed test data into the Accounts and MeterReadings tables
            await connection.ExecuteAsync("INSERT INTO Accounts (AccountId, FirstName, LastName) VALUES (1234, 'John', 'Doe')");
            await connection.ExecuteAsync("INSERT INTO MeterReadings (AccountId, MeterReadingValue, ReadingDate) VALUES (1234, 12345, '2019-04-24')");

            // Mock the configuration to return the shared in-memory connection string
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["ConnectionStrings:DefaultConnection"]).Returns(connectionString);

            // Initialize the controller with the mock configuration
            _controller = new MeterReadingController(_configuration);

            // Act
            var result = await _controller.UploadMeterReadings(mockFile.Object) as OkObjectResult;

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            }
        }
    }
}