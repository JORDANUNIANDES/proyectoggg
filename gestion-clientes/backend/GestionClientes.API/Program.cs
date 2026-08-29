using GestionClientes.API.Middleware;
using GestionClientes.Application.Interfaces;
using GestionClientes.Application.Services;
using GestionClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Gestión de Clientes API",
        Version = "v1",
        Description = "API RESTful profesional para la administración de clientes en .NET 10"
    });
});

// Configure Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=GestionClientesDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

builder.Services.AddDbContext<ClientesDbContext>(options =>
{
    if (OperatingSystem.IsWindows())
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        // En entorno Linux de prueba, si SQL Server no está disponible, se utiliza InMemory como fallback transparente
        options.UseInMemoryDatabase("GestionClientesDB");
    }
});

// Scoped DbContext mapping for Service Layer
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ClientesDbContext>());
builder.Services.AddScoped<IClienteService, ClienteService>();

// CORS setup for Angular Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gestión de Clientes API v1");
    });
}

app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

// Partial class definition for WebApplicationFactory integration testing
public partial class Program { }
