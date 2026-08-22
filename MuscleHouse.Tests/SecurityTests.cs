using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;
using MuscleHouse.Services;
using Xunit;

namespace MuscleHouse.Tests
{
    public class SecurityTests
    {
        private DbContextOptions<ApplicationDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Client_CanAccessOwnData_ButNotOtherClientData()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var securityService = new SecurityService(context);

            var user1 = new ApplicationUser { Id = "user-id-1", Email = "juan@test.com" };
            var user2 = new ApplicationUser { Id = "user-id-2", Email = "sofia@test.com" };
            context.Users.AddRange(user1, user2);

            var cliente1 = new Cliente
            {
                Id = 101,
                UserId = user1.Id,
                Nombre = "Juan",
                Apellido = "Perez",
                Activo = true
            };
            var cliente2 = new Cliente
            {
                Id = 102,
                UserId = user2.Id,
                Nombre = "Sofia",
                Apellido = "Rodriguez",
                Activo = true
            };
            context.Clientes.AddRange(cliente1, cliente2);
            await context.SaveChangesAsync();

            // Act & Assert
            // Juan (user-id-1) accessing Juan's data (101) -> True
            bool canAccessOwn = await securityService.CanClientAccessAsync("user-id-1", 101);
            Assert.True(canAccessOwn);

            // Juan (user-id-1) accessing Sofia's data (102) -> False
            bool canAccessOther = await securityService.CanClientAccessAsync("user-id-1", 102);
            Assert.False(canAccessOther);

            // Sofia (user-id-2) accessing Sofia's data (102) -> True
            bool canAccessOwn2 = await securityService.CanClientAccessAsync("user-id-2", 102);
            Assert.True(canAccessOwn2);
        }

        [Fact]
        public async Task Trainer_CanOnlyAccessAssignedClients()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var securityService = new SecurityService(context);

            var trainerUser = new ApplicationUser { Id = "trainer-user-1", Email = "coach@test.com" };
            var trainerUser2 = new ApplicationUser { Id = "trainer-user-2", Email = "coach2@test.com" };
            context.Users.AddRange(trainerUser, trainerUser2);

            var trainer1 = new Entrenador { Id = 10, UserId = trainerUser.Id, Nombre = "Carlos", Activo = true };
            var trainer2 = new Entrenador { Id = 20, UserId = trainerUser2.Id, Nombre = "Maria", Activo = true };
            context.Entrenadores.AddRange(trainer1, trainer2);

            var cliente1 = new Cliente
            {
                Id = 201,
                UserId = "user-client-1",
                Nombre = "Juan",
                EntrenadorId = trainer1.Id,
                Activo = true
            };
            var cliente2 = new Cliente
            {
                Id = 202,
                UserId = "user-client-2",
                Nombre = "Sofia",
                EntrenadorId = trainer2.Id,
                Activo = true
            };
            context.Clientes.AddRange(cliente1, cliente2);
            await context.SaveChangesAsync();

            // Act & Assert
            // Carlos (trainer-user-1) accessing his assigned client Juan (201) -> True
            bool trainer1AccessAssigned = await securityService.CanTrainerAccessAsync("trainer-user-1", 201);
            Assert.True(trainer1AccessAssigned);

            // Carlos (trainer-user-1) accessing Sofia (202) who is assigned to Maria (20) -> False
            bool trainer1AccessUnassigned = await securityService.CanTrainerAccessAsync("trainer-user-1", 202);
            Assert.False(trainer1AccessUnassigned);

            // Maria (trainer-user-2) accessing Sofia (202) -> True
            bool trainer2AccessAssigned = await securityService.CanTrainerAccessAsync("trainer-user-2", 202);
            Assert.True(trainer2AccessAssigned);
        }

        [Fact]
        public async Task Trainer_CanOnlyAccessTheirOwnRutinas()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var securityService = new SecurityService(context);

            var trainerUser = new ApplicationUser { Id = "trainer-user-1", Email = "coach@test.com" };
            var trainerUser2 = new ApplicationUser { Id = "trainer-user-2", Email = "coach2@test.com" };
            context.Users.AddRange(trainerUser, trainerUser2);

            var trainer1 = new Entrenador { Id = 10, UserId = trainerUser.Id, Nombre = "Carlos", Activo = true };
            var trainer2 = new Entrenador { Id = 20, UserId = trainerUser2.Id, Nombre = "Maria", Activo = true };
            context.Entrenadores.AddRange(trainer1, trainer2);

            var rutina1 = new Rutina { Id = 301, ClienteId = 1, EntrenadorId = trainer1.Id, Nombre = "Rutina A" };
            var rutina2 = new Rutina { Id = 302, ClienteId = 2, EntrenadorId = trainer2.Id, Nombre = "Rutina B" };
            context.Rutinas.AddRange(rutina1, rutina2);
            await context.SaveChangesAsync();

            // Act & Assert
            // Carlos accessing his Routine 301 -> True
            bool canAccessOwn = await securityService.CanTrainerAccessRutinaAsync("trainer-user-1", 301);
            Assert.True(canAccessOwn);

            // Carlos accessing Maria's Routine 302 -> False
            bool canAccessOther = await securityService.CanTrainerAccessRutinaAsync("trainer-user-1", 302);
            Assert.False(canAccessOther);
        }
    }
}
