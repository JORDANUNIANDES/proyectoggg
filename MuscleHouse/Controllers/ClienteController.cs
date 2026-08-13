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
    [Authorize(Roles = "Usuario")]
    public class ClienteController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly IMembershipService _membershipService;
        private readonly IPaymentService _paymentService;
        private readonly IAttendanceService _attendanceService;
        private readonly IWorkoutService _workoutService;
        private readonly IProgressService _progressService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public ClienteController(
            IStaffService staffService,
            IMembershipService membershipService,
            IPaymentService paymentService,
            IAttendanceService attendanceService,
            IWorkoutService workoutService,
            IProgressService progressService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            _staffService = staffService;
            _membershipService = membershipService;
            _paymentService = paymentService;
            _attendanceService = attendanceService;
            _workoutService = workoutService;
            _progressService = progressService;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        private string GetUserId() => _userManager.GetUserId(User) ?? string.Empty;

        private async Task<Cliente?> GetCurrentClienteAsync()
        {
            var userId = GetUserId();
            return await _staffService.GetClienteByUserIdAsync(userId);
        }

        public async Task<IActionResult> Dashboard()
        {
            var cliente = await GetCurrentClienteAsync();
            if (cliente == null)
            {
                return NotFound("No se encontró su perfil de cliente de MUSCLE HOUSE.");
            }

            var activeMembresia = await _membershipService.GetActiveMembershipAsync(cliente.Id);
            var activeRoutine = await _workoutService.GetActiveRoutineAsync(cliente.Id);
            var history = await _progressService.GetClientProgressHistoryAsync(cliente.Id);
            var attendance = await _attendanceService.GetClientAttendanceAsync(cliente.Id);

            int remainingDays = 0;
            if (activeMembresia != null)
            {
                remainingDays = Math.Max(0, (activeMembresia.FechaVencimiento.Date - DateTime.Today).Days);
            }

            var model = new ClienteDashboardViewModel
            {
                Cliente = cliente,
                MembresiaActual = activeMembresia,
                DiasRestantes = remainingDays,
                Entrenador = cliente.Entrenador,
                RutinaActiva = activeRoutine,
                UltimasAsistencias = attendance.Take(5).ToList(),
                HistorialProgreso = history.ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var cliente = await GetCurrentClienteAsync();
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpGet]
        public async Task<IActionResult> Membresia()
        {
            var cliente = await GetCurrentClienteAsync();
            if (cliente == null) return NotFound();

            var memberships = await _membershipService.GetClientMembershipsAsync(cliente.Id);
            ViewBag.Active = await _membershipService.GetActiveMembershipAsync(cliente.Id);
            return View(memberships);
        }

        [HttpGet]
        public async Task<IActionResult> Pagos()
        {
            var cliente = await GetCurrentClienteAsync();
            if (cliente == null) return NotFound();

            var payments = await _paymentService.GetClientPaymentsAsync(cliente.Id);
            return View(payments);
        }

        [HttpGet]
        public async Task<IActionResult> Asistencias()
        {
            var cliente = await GetCurrentClienteAsync();
            if (cliente == null) return NotFound();

            var attendances = await _attendanceService.GetClientAttendanceAsync(cliente.Id);
            return View(attendances);
        }

        [HttpGet]
        public async Task<IActionResult> Rutina()
        {
            var cliente = await GetCurrentClienteAsync();
            if (cliente == null) return NotFound();

            var activeRoutine = await _workoutService.GetActiveRoutineAsync(cliente.Id);
            var routinesHistory = await _workoutService.GetClientRoutinesAsync(cliente.Id);

            ViewBag.History = routinesHistory;
            return View(activeRoutine);
        }

        [HttpGet]
        public async Task<IActionResult> Progreso()
        {
            var cliente = await GetCurrentClienteAsync();
            if (cliente == null) return NotFound();

            var progress = await _progressService.GetClientProgressHistoryAsync(cliente.Id);
            return View(progress);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarProgresoPropio(RegisterProgresoViewModel model)
        {
            var cliente = await GetCurrentClienteAsync();
            if (cliente == null || cliente.Id != model.ClienteId)
            {
                return Forbid("No tiene autorización para registrar progreso para este cliente.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Medidas inválidas. Intente nuevamente.";
                return RedirectToAction(nameof(Dashboard));
            }

            await _progressService.RegisterProgressAsync(
                model.ClienteId, model.Peso, model.Pecho, model.Cintura, model.Brazo, model.Pierna, model.Cadera, model.Observaciones);

            TempData["SuccessMessage"] = "¡Tu progreso ha sido registrado con éxito!";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> Notificaciones()
        {
            var userId = GetUserId();
            var notifications = await _dbContext.Notificaciones
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Fecha)
                .ToListAsync();
            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarNotificacionLeida(int id)
        {
            var userId = GetUserId();
            var notif = await _dbContext.Notificaciones.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (notif == null)
            {
                return NotFound("Notificación no encontrada o no autorizada.");
            }

            notif.Leida = true;
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Notificación marcada como leída.";
            return RedirectToAction(nameof(Notificaciones));
        }
    }
}
