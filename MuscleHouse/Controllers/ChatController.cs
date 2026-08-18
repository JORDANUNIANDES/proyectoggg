using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MuscleHouse.Models;
using MuscleHouse.Services;
using MuscleHouse.ViewModels;

namespace MuscleHouse.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class ChatController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly IAIContextService _aiContextService;
        private readonly IAIService _aiService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(
            IStaffService staffService,
            IAIContextService aiContextService,
            IAIService aiService,
            UserManager<ApplicationUser> userManager)
        {
            _staffService = staffService;
            _aiContextService = aiContextService;
            _aiService = aiService;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User) ?? string.Empty;

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage([FromBody] ChatQueryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("El mensaje no puede estar vacío.");
            }

            var userId = GetUserId();
            var cliente = await _staffService.GetClienteByUserIdAsync(userId);
            if (cliente == null)
            {
                return NotFound("No se encontró su perfil de cliente de MUSCLE HOUSE.");
            }

            // Build client context securely based on authenticated UserId
            var aiContext = await _aiContextService.BuildClientContextAsync(userId);

            // Call IAIService passing client context
            var responseText = await _aiService.ChatAsync(aiContext, model.Message);

            return Json(new { reply = responseText });
        }
    }
}
