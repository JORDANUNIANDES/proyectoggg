using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MuscleHouse.Data;
using MuscleHouse.Models;
using MuscleHouse.Services;
using Xunit;

namespace MuscleHouse.Tests
{
    public class BusinessServicesTests
    {
        private DbContextOptions<ApplicationDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task MembershipService_CanCreateAndRenew_PreservingHistory()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var membershipService = new MembershipService(context);

            var cliente = new Cliente { Id = 1, Nombre = "Juan", Apellido = "Perez", Activo = true };
            var plan = new Plan { Id = 10, Nombre = "Mensual", DuracionDias = 30, Precio = 45.00m };
            context.Clientes.Add(cliente);
            context.Planes.Add(plan);
            await context.SaveChangesAsync();

            // Act - Create first membership
            var m1 = await membershipService.CreateMembershipAsync(cliente.Id, plan.Id, plan.DuracionDias, plan.Precio);

            // Assert m1
            Assert.NotNull(m1);
            Assert.Equal("Activa", m1.Estado);
            Assert.Equal(DateTime.Today, m1.FechaInicio);
            Assert.Equal(DateTime.Today.AddDays(30), m1.FechaVencimiento);

            // Act - Renew membership (extends from first's expiration date)
            var m2 = await membershipService.RenewMembershipAsync(cliente.Id, plan.Id, plan.DuracionDias, plan.Precio);

            // Assert history is preserved
            var list = (await membershipService.GetClientMembershipsAsync(cliente.Id)).ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal(m1.FechaVencimiento, m2.FechaInicio);
            Assert.Equal(m1.FechaVencimiento.AddDays(30), m2.FechaVencimiento);
        }

        [Fact]
        public async Task PaymentService_CanRegisterAndRetrieve_Payments()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var paymentService = new PaymentService(context);

            var cliente = new Cliente { Id = 1, Nombre = "Juan", Activo = true };
            var plan = new Plan { Id = 5, Nombre = "Anual", DuracionDias = 365, Precio = 260.00m };
            var membresia = new Membresia { Id = 50, ClienteId = cliente.Id, PlanId = plan.Id, FechaInicio = DateTime.Today, FechaVencimiento = DateTime.Today.AddDays(365), PrecioPagado = 260.00m, Estado = "Activa" };
            context.Clientes.Add(cliente);
            context.Planes.Add(plan);
            context.Membresias.Add(membresia);
            await context.SaveChangesAsync();

            // Act
            var pago = await paymentService.RegisterPaymentAsync(cliente.Id, membresia.Id, 260.00m, "Tarjeta");

            // Assert
            Assert.NotNull(pago);
            Assert.Equal(260.00m, pago.Monto);
            Assert.Equal("Tarjeta", pago.MetodoPago);
            Assert.Equal("Completado", pago.Estado);

            var retrieved = (await paymentService.GetClientPaymentsAsync(cliente.Id)).ToList();
            Assert.Single(retrieved);
            Assert.Equal(260.00m, retrieved[0].Monto);
        }

        [Fact]
        public async Task AttendanceService_CanRegisterAndRetrieve_Attendance()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var attendanceService = new AttendanceService(context);

            var cliente = new Cliente { Id = 1, Nombre = "Juan", Activo = true };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            // Act
            var attendance = await attendanceService.RegisterAttendanceAsync(cliente.Id);

            // Assert
            Assert.NotNull(attendance);
            Assert.True(attendance.FechaHora <= DateTime.Now);

            var history = (await attendanceService.GetClientAttendanceAsync(cliente.Id)).ToList();
            Assert.Single(history);
        }

        [Fact]
        public async Task WorkoutService_TrainerUnassigned_ThrowsUnauthorized()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var workoutService = new WorkoutService(context);

            var trainerUser = new ApplicationUser { Id = "trainer-user", Email = "coach@musclehouse.com" };
            var trainer = new Entrenador { Id = 1, UserId = trainerUser.Id, Nombre = "Carlos", Activo = true };
            context.Users.Add(trainerUser);
            context.Entrenadores.Add(trainer);

            var cliente = new Cliente { Id = 10, Nombre = "Juan", EntrenadorId = 999, Activo = true }; // assigned to another trainer ID 999
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                workoutService.CreateRoutineAsync("trainer-user", cliente.Id, "Rutina de Fuerza", new List<RutinaEjercicio>()));
        }

        [Fact]
        public async Task MockAIService_AnalyzesRealDataAndReturnsSpanishResponse()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var mockAI = new MockAIService(context);

            var user = new ApplicationUser { Id = "client-user", Email = "client@musclehouse.com" };
            context.Users.Add(user);

            var cliente = new Cliente { Id = 1, UserId = user.Id, Nombre = "Juan", Objetivo = "Aumento de masa muscular", Activo = true };
            context.Clientes.Add(cliente);

            var progreso = new Progreso { Id = 1, ClienteId = cliente.Id, Fecha = DateTime.Today, Peso = 82.50m, Pecho = 102m, Cintura = 84m, Brazo = 36m, Pierna = 58m, Cadera = 97m, Observaciones = "Fuerza máxima" };
            context.Progresos.Add(progreso);
            await context.SaveChangesAsync();

            // Act
            var reply = await mockAI.ChatAsync("client-user", "Hola, ¿cómo voy con mi progreso?");

            // Assert
            Assert.NotNull(reply);
            Assert.Contains("Juan", reply);
            Assert.Contains("Aumento de masa muscular", reply);
            Assert.Contains("82.50", reply);
            Assert.Contains("Brazo", reply);
            Assert.Contains("observaciones", reply, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task OpenAIAIService_WhenApiKeyIsAbsent_FallsBackToMockAIService()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var mockAI = new MockAIService(context);
            var inMemoryConfig = new Dictionary<string, string?> {
                {"OpenAI:ApiKey", null} // Key is absent
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();
            var httpClient = new HttpClient();

            var user = new ApplicationUser { Id = "client-user", Email = "client@musclehouse.com" };
            context.Users.Add(user);

            var cliente = new Cliente { Id = 1, UserId = user.Id, Nombre = "Juan", Objetivo = "Tonificación y resistencia", Activo = true };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            var openAI = new OpenAIAIService(config, httpClient, mockAI);

            // Act
            var reply = await openAI.ChatAsync("client-user", "Recomendación para tonificar");

            // Assert (returns mock fallback output containing real data)
            Assert.NotNull(reply);
            Assert.Contains("Juan", reply);
            Assert.Contains("Tonificación y resistencia", reply);
        }
    }
}
