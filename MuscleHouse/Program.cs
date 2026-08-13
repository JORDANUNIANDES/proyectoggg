using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure ApplicationDbContext to use exclusively SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=GymManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add ASP.NET Core Identity with standard security policy (suitable for production/development)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
});

// Register custom services
builder.Services.AddScoped<MuscleHouse.Services.ISecurityService, MuscleHouse.Services.SecurityService>();
builder.Services.AddScoped<MuscleHouse.Services.IMembershipService, MuscleHouse.Services.MembershipService>();
builder.Services.AddScoped<MuscleHouse.Services.IPaymentService, MuscleHouse.Services.PaymentService>();
builder.Services.AddScoped<MuscleHouse.Services.IAttendanceService, MuscleHouse.Services.AttendanceService>();
builder.Services.AddScoped<MuscleHouse.Services.IWorkoutService, MuscleHouse.Services.WorkoutService>();
builder.Services.AddScoped<MuscleHouse.Services.IProgressService, MuscleHouse.Services.ProgressService>();
builder.Services.AddScoped<MuscleHouse.Services.IStaffService, MuscleHouse.Services.StaffService>();

// Register AI Services and HttpClient
builder.Services.AddHttpClient();
builder.Services.AddScoped<MuscleHouse.Services.MockAIService>();
builder.Services.AddScoped<MuscleHouse.Services.IAIService, MuscleHouse.Services.OpenAIAIService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed the database automatically at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar o sembrar la base de datos.");
    }
}

app.Run();
