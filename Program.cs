using MeterDataLayer;
using Microsoft.EntityFrameworkCore;
using MeterShared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<MeterReadingDataBase>(options =>
//options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<MeterReadingDataBase>();
//    await SeedTestAccountData(dbContext, "./SolutionItems/Test_Accounts.ods");
//}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbInitialiser = new DatabaseInitialiser(connectionString);
dbInitialiser.Initialise();


async Task SeedTestAccountData(MeterReadingDataBase dbContext, string filePath)
{
    if (!dbContext.Accounts.Any())
    {
        var accounts = File.ReadAllLines(filePath)
            .Skip(1)
            .Select(line =>
            {
                var data = line.Split(',');

                return new Account()
                {
                    AccountId = int.Parse(data[0]),
                    FirstName = data[1],
                    LastName = data[2]
                };
            });

        await dbContext.Accounts.AddRangeAsync(accounts);
        await dbContext.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
