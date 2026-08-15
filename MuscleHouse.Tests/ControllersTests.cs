using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Identity;
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
    public class ControllersTests
    {
        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            return mgr;
        }

        private Mock<SignInManager<ApplicationUser>> GetMockSignInManager(Mock<UserManager<ApplicationUser>> userManagerMock)
        {
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                null!, null!, null!, null!);

            return signInManagerMock;
        }

        [Fact]
        public async Task AccountController_Register_CreatesUserWithUsuarioRoleAndRedirectsToClienteDashboard()
        {
            // Arrange
            var userMgrMock = GetMockUserManager();
            var signInMgrMock = GetMockSignInManager(userMgrMock);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new ApplicationDbContext(options);

            userMgrMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            userMgrMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            userMgrMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Usuario"))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AccountController(signInMgrMock.Object, userMgrMock.Object, dbContext);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var model = new RegisterViewModel
            {
                Email = "nuevo_usuario@test.com",
                Password = "MusclePassword123!",
                ConfirmPassword = "MusclePassword123!",
                Nombre = "Pedro",
                Apellido = "Gomez",
                Telefono = "7000-1111",
                FechaNacimiento = DateTime.Today.AddYears(-22),
                Objetivo = "Ganar masa muscular"
            };

            // Act
            var result = await controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal("Cliente", redirectResult.ControllerName);

            userMgrMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Usuario"), Times.Once);

            var createdCliente = await dbContext.Clientes.FirstOrDefaultAsync(c => c.Nombre == "Pedro");
            Assert.NotNull(createdCliente);
            Assert.Equal("Gomez", createdCliente.Apellido);
        }

        [Fact]
        public async Task AccountController_Register_DuplicateEmail_ReturnsViewWithModelError()
        {
            // Arrange
            var userMgrMock = GetMockUserManager();
            var signInMgrMock = GetMockSignInManager(userMgrMock);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new ApplicationDbContext(options);

            userMgrMock.Setup(m => m.FindByEmailAsync("existente@test.com"))
                .ReturnsAsync(new ApplicationUser { Email = "existente@test.com" });

            var controller = new AccountController(signInMgrMock.Object, userMgrMock.Object, dbContext);

            var model = new RegisterViewModel
            {
                Email = "existente@test.com",
                Password = "MusclePassword123!",
                ConfirmPassword = "MusclePassword123!",
                Nombre = "Pedro",
                Apellido = "Gomez",
                Telefono = "7000-1111",
                FechaNacimiento = DateTime.Today.AddYears(-22),
                Objetivo = "Pérdida de peso"
            };

            // Act
            var result = await controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Email"));
        }

        [Fact]
        public async Task ChatController_SendMessage_ReturnsCorrectJsonReply()
        {
            // Arrange
            var mockStaff = new Mock<IStaffService>();
            var mockAI = new Mock<IAIService>();
            var mockUserMgr = GetMockUserManager();

            var user = new ApplicationUser { Id = "client-user-123", Email = "juan@test.com" };
            var cliente = new Cliente { Id = 1, UserId = user.Id, Nombre = "Juan" };

            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id);
            mockStaff.Setup(s => s.GetClienteByUserIdAsync(user.Id)).ReturnsAsync(cliente);
            mockAI.Setup(ai => ai.ChatAsync(user.Id, "Hola")).ReturnsAsync("Respuesta de IA de prueba");

            var controller = new ChatController(mockStaff.Object, mockAI.Object, mockUserMgr.Object);

            // Mock User identity for controller
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var model = new ChatQueryViewModel { Message = "Hola" };

            // Act
            var result = await controller.SendMessage(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            object? dataObj = jsonResult.Value;
            Assert.NotNull(dataObj);

            // Extract property from anonymous type using reflection safely
            var replyProp = dataObj.GetType().GetProperty("reply");
            Assert.NotNull(replyProp);
            var replyValue = replyProp.GetValue(dataObj, null);
            Assert.Equal("Respuesta de IA de prueba", replyValue);
        }

        [Fact]
        public async Task ChatController_SendMessage_EmptyMessage_ReturnsBadRequest()
        {
            // Arrange
            var mockStaff = new Mock<IStaffService>();
            var mockAI = new Mock<IAIService>();
            var mockUserMgr = GetMockUserManager();

            var controller = new ChatController(mockStaff.Object, mockAI.Object, mockUserMgr.Object);
            controller.ModelState.AddModelError("Message", "El mensaje no puede estar vacío.");

            var model = new ChatQueryViewModel { Message = "" };

            // Act
            var result = await controller.SendMessage(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task EntrenadorController_CrearRutina_UnauthorizedTrainer_ReturnsForbid()
        {
            // Arrange
            var mockStaff = new Mock<IStaffService>();
            var mockWorkout = new Mock<IWorkoutService>();
            var mockProgress = new Mock<IProgressService>();
            var mockSecurity = new Mock<ISecurityService>();
            var mockUserMgr = GetMockUserManager();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new ApplicationDbContext(options);

            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("trainer-id-99");
            mockSecurity.Setup(s => s.CanTrainerAccessAsync("trainer-id-99", 5)).ReturnsAsync(false); // No access

            var controller = new EntrenadorController(
                mockStaff.Object, mockWorkout.Object, mockProgress.Object, mockSecurity.Object, mockUserMgr.Object, dbContext);

            var model = new CreateRutinaViewModel { ClienteId = 5, Nombre = "Rutina Ilegal" };

            // Act
            var result = await controller.CrearRutina(model);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
