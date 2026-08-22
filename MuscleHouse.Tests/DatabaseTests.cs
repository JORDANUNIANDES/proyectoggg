using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MuscleHouse.Data;
using MuscleHouse.Models;
using Xunit;

namespace MuscleHouse.Tests
{
    public class DatabaseTests
    {
        private DbContextOptions<ApplicationDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void DbContext_CanBeInitialized_AndCanPersistEntities()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);

            // Act & Assert
            context.Database.EnsureCreated();

            var plan = new Plan
            {
                Nombre = "Mensual",
                DuracionDias = 30,
                Precio = 45.00m,
                Descripcion = "Acceso completo por un mes"
            };

            context.Planes.Add(plan);
            context.SaveChanges();

            Assert.True(plan.Id > 0);
            Assert.Equal("Mensual", context.Planes.First().Nombre);
        }

        [Fact]
        public void DbContext_CanSaveAndRetrieve_ClienteAndEntrenador()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            // Act
            var user = new ApplicationUser { UserName = "coach@test.com", Email = "coach@test.com" };
            context.Users.Add(user);

            var entrenador = new Entrenador
            {
                UserId = user.Id,
                Nombre = "John",
                Apellido = "Doe",
                Especialidad = "Hipertrofia",
                Experiencia = 5,
                Descripcion = "Experto en culturismo",
                Fotografia = "uploads/coach1.png",
                Activo = true
            };
            context.Entrenadores.Add(entrenador);

            var userCliente = new ApplicationUser { UserName = "client@test.com", Email = "client@test.com" };
            context.Users.Add(userCliente);

            var cliente = new Cliente
            {
                UserId = userCliente.Id,
                Nombre = "Jane",
                Apellido = "Smith",
                Telefono = "12345678",
                FechaNacimiento = new DateTime(1995, 5, 15),
                Objetivo = "Perder peso",
                Entrenador = entrenador,
                Activo = true
            };
            context.Clientes.Add(cliente);
            context.SaveChanges();

            // Assert
            var retrievedCliente = context.Clientes.Include(c => c.Entrenador).FirstOrDefault();
            Assert.NotNull(retrievedCliente);
            Assert.NotNull(retrievedCliente.Entrenador);
            Assert.Equal("John", retrievedCliente.Entrenador.Nombre);
        }

        [Fact]
        public async Task Seeding_IsFullyIdempotent()
        {
            // Arrange
            var dbName = "SeedingTest_" + Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(opt => opt.UseInMemoryDatabase(databaseName: dbName));
            services.AddLogging();
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var provider = services.BuildServiceProvider();

            // Act - Run first time
            await DbInitializer.SeedAsync(provider);

            // Assert first execution
            using (var scope = provider.CreateScope())
            {
                var checkContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Assert.Equal(4, checkContext.Roles.Count());
                Assert.Equal(5, checkContext.Planes.Count());
                Assert.Equal(2, checkContext.Clientes.Count());
                Assert.Equal(2, checkContext.Entrenadores.Count());
                Assert.Equal(1, checkContext.Recepcionistas.Count());
            }

            // Act - Run second time (should NOT duplicate or throw)
            await DbInitializer.SeedAsync(provider);

            // Assert second execution (it should be exactly the same, no duplicates)
            using (var scope = provider.CreateScope())
            {
                var checkContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Assert.Equal(4, checkContext.Roles.Count());
                Assert.Equal(5, checkContext.Planes.Count());
                Assert.Equal(2, checkContext.Clientes.Count());
                Assert.Equal(2, checkContext.Entrenadores.Count());
                Assert.Equal(1, checkContext.Recepcionistas.Count());
            }
        }
    }
}
