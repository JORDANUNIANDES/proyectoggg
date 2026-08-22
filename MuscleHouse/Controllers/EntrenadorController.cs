using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
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
        private readonly ApplicationDbContext _dbContext;

        public EntrenadorController(
            IStaffService staffService,
            IWorkoutService workoutService,
            IProgressService progressService,
            ISecurityService securityService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            _staffService = staffService;
            _workoutService = workoutService;
            _progressService = progressService;
            _securityService = securityService;
            _userManager = userManager;
            _dbContext = dbContext;
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

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var trainerUserId = GetUserId();
            var trainer = await _staffService.GetTrainerByUserIdAsync(trainerUserId);
            if (trainer == null) return NotFound();

            return View(trainer);
        }

        [HttpGet]
        public async Task<IActionResult> Clientes(string? search, string? estado, int page = 1)
        {
            var trainerUserId = GetUserId();
            var trainer = await _staffService.GetTrainerByUserIdAsync(trainerUserId);
            if (trainer == null) return NotFound("Entrenador no encontrado.");

            var query = _dbContext.Clientes
                .Include(c => c.User)
                .Include(c => c.Membresias)
                .ThenInclude(m => m.Plan)
                .Where(c => c.EntrenadorId == trainer.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c => c.Nombre.ToLower().Contains(term) ||
                                         c.Apellido.ToLower().Contains(term) ||
                                         (c.Nombre + " " + c.Apellido).ToLower().Contains(term) ||
                                         c.Telefono.Contains(term) ||
                                         c.Objetivo.ToLower().Contains(term) ||
                                         (c.User != null && c.User.Email != null && c.User.Email.ToLower().Contains(term)));
            }

            if (estado == "Activo") query = query.Where(c => c.Activo);
            else if (estado == "Inactivo") query = query.Where(c => !c.Activo);

            int totalItems = await query.CountAsync();
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var clients = await query.OrderBy(c => c.Nombre).ThenBy(c => c.Apellido)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Estado = estado;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(clients);
        }

        [HttpGet]
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
        public async Task<IActionResult> Rutinas()
        {
            var trainerUserId = GetUserId();
            var trainer = await _staffService.GetTrainerByUserIdAsync(trainerUserId);
            if (trainer == null) return NotFound();

            var rutinas = await _dbContext.Rutinas
                .Include(r => r.Cliente)
                .Include(r => r.RutinaEjercicios)
                .ThenInclude(re => re.Ejercicio)
                .Where(r => r.EntrenadorId == trainer.Id)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return View(rutinas);
        }

        [HttpGet]
        public async Task<IActionResult> Progreso()
        {
            var trainerUserId = GetUserId();
            var clients = await _staffService.GetAssignedClientsAsync(trainerUserId);
            var clientIds = clients.Select(c => c.Id).ToList();

            var progresos = await _dbContext.Progresos
                .Include(p => p.Cliente)
                .Where(p => clientIds.Contains(p.ClienteId))
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return View(progresos);
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
            ViewBag.Ejercicios = await _workoutService.GetAllExercisesAsync();
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
                ViewBag.Ejercicios = await _workoutService.GetAllExercisesAsync();
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
