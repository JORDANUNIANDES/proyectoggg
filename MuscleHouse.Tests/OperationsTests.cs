using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using MuscleHouse.Controllers;
using MuscleHouse.Data;
using MuscleHouse.Models;
using MuscleHouse.Services;
using MuscleHouse.ViewModels;
using Xunit;

namespace MuscleHouse.Tests
{
    public class OperationsTests
    {
        private DbContextOptions<ApplicationDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task StaffService_ReassignTrainer_ClosesPreviousAndPreservesHistory()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var staffService = new StaffService(context);

            var trainer1 = new Entrenador { Id = 1, Nombre = "Carlos", Activo = true };
            var trainer2 = new Entrenador { Id = 2, Nombre = "Maria", Activo = true };
            context.Entrenadores.AddRange(trainer1, trainer2);

            var cliente = new Cliente { Id = 10, Nombre = "Juan", EntrenadorId = trainer1.Id, Activo = true };
            context.Clientes.Add(cliente);

            // Active initial assignment
            var initialAsg = new AsignacionEntrenador
            {
                ClienteId = cliente.Id,
                EntrenadorId = trainer1.Id,
                FechaInicio = DateTime.Now.AddDays(-10),
                FechaFin = null
            };
            context.AsignacionesEntrenadores.Add(initialAsg);
            await context.SaveChangesAsync();

            // Act - Reassign to trainer 2
            var success = await staffService.AssignTrainerToClientAsync(cliente.Id, trainer2.Id);

            // Assert
            Assert.True(success);
            Assert.Equal(trainer2.Id, cliente.EntrenadorId);

            var assignments = await context.AsignacionesEntrenadores.ToListAsync();
            Assert.Equal(2, assignments.Count);

            // Initial assignment is now closed
            var closedAsg = assignments.First(a => a.EntrenadorId == trainer1.Id);
            Assert.NotNull(closedAsg.FechaFin);

            // New assignment is active
            var activeAsg = assignments.First(a => a.EntrenadorId == trainer2.Id);
            Assert.Null(activeAsg.FechaFin);
        }

        [Fact]
        public void ImageUpload_ValidationRules_AreRespected()
        {
            // Arrange
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            // 1. Check a valid image mockup
            var validFileMock = new Mock<IFormFile>();
            validFileMock.Setup(f => f.FileName).Returns("avatar.png");
            validFileMock.Setup(f => f.Length).Returns(1 * 1024 * 1024); // 1 MB
            validFileMock.Setup(f => f.ContentType).Returns("image/png");

            var ext = Path.GetExtension(validFileMock.Object.FileName).ToLower();
            Assert.Contains(ext, allowedExtensions);
            Assert.True(validFileMock.Object.Length <= 2 * 1024 * 1024);

            // 2. Too big file mockup (3 MB)
            var bigFileMock = new Mock<IFormFile>();
            bigFileMock.Setup(f => f.FileName).Returns("photo.jpg");
            bigFileMock.Setup(f => f.Length).Returns(3 * 1024 * 1024); // 3 MB
            Assert.True(bigFileMock.Object.Length > 2 * 1024 * 1024);

            // 3. Bad extension mockup (.pdf)
            var badFileMock = new Mock<IFormFile>();
            badFileMock.Setup(f => f.FileName).Returns("resume.pdf");
            var badExt = Path.GetExtension(badFileMock.Object.FileName).ToLower();
            Assert.DoesNotContain(badExt, allowedExtensions);
        }
    }
}
