using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MuscleHouse.Models;
using MuscleHouse.Services;
using MuscleHouse.ViewModels;

namespace MuscleHouse.Controllers
{
    [Authorize(Roles = "Entrenador")]
    public class EntrenadorController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly IWorkoutService _workoutService;
        private readonly IProgressService _progressService;
        private readonly ISecurityService _securityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EntrenadorController(
            IStaffService staffService,
            IWorkoutService workoutService,
            IProgressService progressService,
            ISecurityService securityService,
            UserManager<ApplicationUser> userManager)
        {
            _staffService = staffService;
            _workoutService = workoutService;
            _progressService = progressService;
            _securityService = securityService;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User) ?? string.Empty;

        public async Task<IActionResult> Dashboard()
        {
            var trainerUserId = GetUserId();
            var clients = await _staffService.GetAssignedClientsAsync(trainerUserId);

            var model = new EntrenadorDashboardViewModel
            {
                TotalClientesAsignados = clients.Count(),
                ClientesAsignados = clients.ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> ClienteDetalles(int id)
        {
            var trainerUserId = GetUserId();
            if (!await _securityService.CanTrainerAccessAsync(trainerUserId, id))
            {
                return Forbid("No tiene autorización para ver los datos de este cliente.");
            }

            var cliente = await _staffService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            var routines = await _workoutService.GetClientRoutinesAsync(id);
            var history = await _progressService.GetClientProgressHistoryAsync(id);
            var logs = await _workoutService.GetWorkoutLogsAsync(id);

            ViewBag.Rutinas = routines;
            ViewBag.Progresos = history;
            ViewBag.Logs = logs;

            return View(cliente);
        }

        [HttpGet]
        public async Task<IActionResult> CrearRutina(int clienteId)
        {
            var trainerUserId = GetUserId();
            if (!await _securityService.CanTrainerAccessAsync(trainerUserId, clienteId))
            {
                return Forbid();
            }

            var model = new CreateRutinaViewModel { ClienteId = clienteId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearRutina(CreateRutinaViewModel model)
        {
            var trainerUserId = GetUserId();
            if (!await _securityService.CanTrainerAccessAsync(trainerUserId, model.ClienteId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var exercises = model.Ejercicios.Select(e => new RutinaEjercicio
            {
                EjercicioId = e.EjercicioId,
                Series = e.Series,
                Repeticiones = e.Repeticiones,
                PesoRecomendado = e.PesoRecomendado,
                DescansoSegundos = e.DescansoSegundos,
                Orden = e.Orden,
                Observaciones = e.Observaciones
            }).ToList();

            await _workoutService.CreateRoutineAsync(trainerUserId, model.ClienteId, model.Nombre, exercises);
            TempData["SuccessMessage"] = "¡Rutina creada con éxito!";
            return RedirectToAction(nameof(ClienteDetalles), new { id = model.ClienteId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarProgreso(RegisterProgresoViewModel model)
        {
            var trainerUserId = GetUserId();
            if (!await _securityService.CanTrainerAccessAsync(trainerUserId, model.ClienteId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Medidas inválidas. Verifique los datos.";
                return RedirectToAction(nameof(ClienteDetalles), new { id = model.ClienteId });
            }

            await _progressService.RegisterProgressAsync(
                model.ClienteId, model.Peso, model.Pecho, model.Cintura, model.Brazo, model.Pierna, model.Cadera, model.Observaciones);

            TempData["SuccessMessage"] = "¡Progreso registrado con éxito!";
            return RedirectToAction(nameof(ClienteDetalles), new { id = model.ClienteId });
        }
    }
}
