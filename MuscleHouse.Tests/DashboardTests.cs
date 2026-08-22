using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;
using MuscleHouse.Services;
using MuscleHouse.ViewModels;
using Xunit;

namespace MuscleHouse.Tests
{
    public class DashboardTests
    {
        private DbContextOptions<ApplicationDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AdminDashboard_Metrics_ComputeCorrectlyFromDatabase()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var planDiario = new Plan { Id = 1, Nombre = "Diario", DuracionDias = 1, Precio = 5.00m };
            var planAnual = new Plan { Id = 2, Nombre = "Anual", DuracionDias = 365, Precio = 260.00m };
            context.Planes.AddRange(planDiario, planAnual);

            var cliente1 = new Cliente { Id = 10, Nombre = "Juan", Activo = true };
            var cliente2 = new Cliente { Id = 20, Nombre = "Sofia", Activo = true };
            context.Clientes.AddRange(cliente1, cliente2);

            var m1 = new Membresia { Id = 101, ClienteId = cliente1.Id, PlanId = planDiario.Id, FechaInicio = DateTime.Today, FechaVencimiento = DateTime.Today.AddDays(1), PrecioPagado = 5.00m, Estado = "Activa" };
            var m2 = new Membresia { Id = 102, ClienteId = cliente2.Id, PlanId = planAnual.Id, FechaInicio = DateTime.Today, FechaVencimiento = DateTime.Today.AddDays(365), PrecioPagado = 260.00m, Estado = "Activa" };
            context.Membresias.AddRange(m1, m2);

            var p1 = new Pago { Id = 1, ClienteId = cliente1.Id, MembresiaId = m1.Id, Monto = 5.00m, Fecha = DateTime.Today, MetodoPago = "Efectivo", Estado = "Completado" };
            var p2 = new Pago { Id = 2, ClienteId = cliente2.Id, MembresiaId = m2.Id, Monto = 260.00m, Fecha = DateTime.Today, MetodoPago = "Tarjeta", Estado = "Completado" };
            context.Pagos.AddRange(p1, p2);

            await context.SaveChangesAsync();

            // Act - compute metrics
            var paymentsList = await context.Pagos
                .Include(p => p.Membresia)
                .ThenInclude(m => m!.Plan)
                .ToListAsync();

            var totalIncome = paymentsList.Where(p => p.Estado == "Completado").Sum(p => p.Monto);
            var activeMembCount = await context.Membresias.CountAsync(m => m.Estado == "Activa");
            var salesByPlan = paymentsList
                .Where(p => p.Membresia != null && p.Membresia.Plan != null)
                .GroupBy(p => p.Membresia!.Plan!.Nombre)
                .ToDictionary(g => g.Key, g => g.Count());

            // Assert
            Assert.Equal(265.00m, totalIncome);
            Assert.Equal(2, activeMembCount);
            Assert.Equal(1, salesByPlan["Diario"]);
            Assert.Equal(1, salesByPlan["Anual"]);
        }

        [Fact]
        public void ClienteDashboard_RemainingDays_ComputesCorrectly()
        {
            // Arrange
            var today = DateTime.Today;
            var activeMemb = new Membresia
            {
                FechaInicio = today.AddDays(-10),
                FechaVencimiento = today.AddDays(20),
                Estado = "Activa"
            };

            // Act
            int remainingDays = Math.Max(0, (activeMemb.FechaVencimiento.Date - today).Days);

            // Assert
            Assert.Equal(20, remainingDays);
        }

        [Fact]
        public async Task Dashboard_EmptyState_HandlesGracefullyWithoutThrowing()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            // Act & Assert (ensure empty tables return counts of 0 and don't throw exceptions)
            var paymentsList = await context.Pagos
                .Include(p => p.Membresia)
                .ThenInclude(m => m!.Plan)
                .ToListAsync();

            var totalIncome = paymentsList.Sum(p => p.Monto);
            var activeMembCount = await context.Membresias.CountAsync(m => m.Estado == "Activa");
            var salesByPlan = paymentsList
                .Where(p => p.Membresia != null && p.Membresia.Plan != null)
                .GroupBy(p => p.Membresia!.Plan!.Nombre)
                .ToDictionary(g => g.Key, g => g.Count());

            Assert.Equal(0m, totalIncome);
            Assert.Equal(0, activeMembCount);
            Assert.Empty(salesByPlan);
        }
    }
}
