using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MuscleHouse.Controllers;
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
            dynamic? data = jsonResult.Value;
            Assert.NotNull(data);

            // Extract property from anonymous type using reflection
            var replyProp = data.GetType().GetProperty("reply");
            Assert.NotNull(replyProp);
            var replyValue = replyProp.GetValue(data, null);
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

            mockUserMgr.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("trainer-id-99");
            mockSecurity.Setup(s => s.CanTrainerAccessAsync("trainer-id-99", 5)).ReturnsAsync(false); // No access

            var controller = new EntrenadorController(
                mockStaff.Object, mockWorkout.Object, mockProgress.Object, mockSecurity.Object, mockUserMgr.Object);

            var model = new CreateRutinaViewModel { ClienteId = 5, Nombre = "Rutina Ilegal" };

            // Act
            var result = await controller.CrearRutina(model);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
