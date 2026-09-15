using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers with JSON Enum String conversion
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure EF Core (SQL Server by default on Windows / LocalDB, SQLite on Linux container dev)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=RestaurantManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && connectionString.Contains("(localdb)"))
{
    // Fallback to SQLite database file for Linux sandbox compatibility
    builder.Services.AddDbContext<RestaurantDbContext>(options =>
        options.UseSqlite("Data Source=RestaurantManagement.db"));
}
else
{
    builder.Services.AddDbContext<RestaurantDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddOpenApi();

var app = builder.Build();

// Enable Static Files for HTML5/CSS3/JS Vanilla frontend in wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();
app.MapControllers();

// Ensure Database Created and Seeded at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
    try
    {
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization notice: {ex.Message}");
    }
}

app.Run();
