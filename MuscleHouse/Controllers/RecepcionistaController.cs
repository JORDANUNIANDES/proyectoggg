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
    [Authorize(Roles = "Recepcionista")]
    public class RecepcionistaController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly IMembershipService _membershipService;
        private readonly IPaymentService _paymentService;
        private readonly IAttendanceService _attendanceService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public RecepcionistaController(
            IStaffService staffService,
            IMembershipService membershipService,
            IPaymentService paymentService,
            IAttendanceService attendanceService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            _staffService = staffService;
            _membershipService = membershipService;
            _paymentService = paymentService;
            _attendanceService = attendanceService;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Dashboard()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            var attendance = await _attendanceService.GetAllAttendanceAsync();

            var totalActiveClients = await _dbContext.Clientes
                .CountAsync(c => c.Activo && c.Membresias.Any(m => m.Estado == "Activa" && m.FechaVencimiento >= DateTime.Today));

            var model = new RecepcionistaDashboardViewModel
            {
                TotalClientesActivos = totalActiveClients,
                IngresosTotales = payments.Sum(p => p.Monto),
                PagosRecientes = payments.Take(5).ToList(),
                AsistenciasDeHoy = attendance.Where(a => a.FechaHora.Date == DateTime.Today).ToList(),
                MembresiasProximasAVencer = await _dbContext.Membresias
                    .Include(m => m.Cliente)
                    .Include(m => m.Plan)
                    .Where(m => m.Estado == "Activa" && m.FechaVencimiento >= DateTime.Today && m.FechaVencimiento <= DateTime.Today.AddDays(7))
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Clientes()
        {
            var clients = await _dbContext.Clientes
                .Include(c => c.User)
                .Include(c => c.Entrenador)
                .Include(c => c.Membresias)
                .ThenInclude(m => m.Plan)
                .ToListAsync();
            return View(clients);
        }

        [HttpGet]
        public async Task<IActionResult> CrearCliente()
        {
            ViewBag.Trainers = await _staffService.GetAllTrainersAsync();
            ViewBag.Plans = await _dbContext.Planes.Where(p => p.Activo).ToListAsync();
            return View(new CreateClienteViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCliente(CreateClienteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Trainers = await _staffService.GetAllTrainersAsync();
                ViewBag.Plans = await _dbContext.Planes.Where(p => p.Activo).ToListAsync();
                return View(model);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                ViewBag.Trainers = await _staffService.GetAllTrainersAsync();
                ViewBag.Plans = await _dbContext.Planes.Where(p => p.Activo).ToListAsync();
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Usuario");

            var cliente = new Cliente
            {
                UserId = user.Id,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Telefono = model.Telefono,
                FechaNacimiento = model.FechaNacimiento,
                Objetivo = model.Objetivo,
                EntrenadorId = model.EntrenadorId,
                Activo = true
            };

            _dbContext.Clientes.Add(cliente);
            await _dbContext.SaveChangesAsync();

            if (model.EntrenadorId.HasValue)
            {
                var newAsg = new AsignacionEntrenador
                {
                    ClienteId = cliente.Id,
                    EntrenadorId = model.EntrenadorId.Value,
                    FechaInicio = DateTime.Now,
                    FechaFin = null
                };
                _dbContext.AsignacionesEntrenadores.Add(newAsg);
                await _dbContext.SaveChangesAsync();
            }

            if (model.PlanId.HasValue)
            {
                var plan = await _dbContext.Planes.FindAsync(model.PlanId.Value);
                if (plan != null)
                {
                    var newMemb = await _membershipService.RenewMembershipAsync(cliente.Id, plan.Id, plan.DuracionDias, plan.Precio);
                    await _paymentService.RegisterPaymentAsync(cliente.Id, newMemb.Id, plan.Precio, "Efectivo");
                }
            }

            var notif = new Notificacion
            {
                UserId = user.Id,
                Titulo = "¡Bienvenido a MUSCLE HOUSE!",
                Mensaje = "Tu cuenta ha sido creada con éxito. ¡Prepárate para dar el 100%!",
                Fecha = DateTime.Now,
                Leida = false
            };
            _dbContext.Notificaciones.Add(notif);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"¡Cliente {model.Nombre} {model.Apellido} registrado con éxito!";
            return RedirectToAction(nameof(Clientes));
        }

        [HttpGet]
        public async Task<IActionResult> Membresias()
        {
            var membresias = await _dbContext.Membresias
                .Include(m => m.Cliente)
                .Include(m => m.Plan)
                .OrderByDescending(m => m.FechaInicio)
                .ToListAsync();
            ViewBag.Clients = await _dbContext.Clientes.Where(c => c.Activo).ToListAsync();
            ViewBag.Planes = await _dbContext.Planes.Where(p => p.Activo).ToListAsync();
            return View(membresias);
        }

        [HttpGet]
        public async Task<IActionResult> Pagos()
        {
            var pagos = await _paymentService.GetAllPaymentsAsync();
            return View(pagos);
        }

        [HttpGet]
        public async Task<IActionResult> Asistencias()
        {
            var asistencias = await _attendanceService.GetAllAttendanceAsync();
            ViewBag.Clientes = await _dbContext.Clientes.Where(c => c.Activo).ToListAsync();
            return View(asistencias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarAsistencia(int clienteId)
        {
            var cliente = await _staffService.GetClienteByIdAsync(clienteId);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            await _attendanceService.RegisterAttendanceAsync(clienteId);
            TempData["SuccessMessage"] = $"¡Asistencia registrada para {cliente.Nombre} {cliente.Apellido}!";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenovarMembresia(RenewMembresiaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cliente = await _staffService.GetClienteByIdAsync(model.ClienteId);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            var plan = await _dbContext.Planes.FindAsync(model.PlanId);
            if (plan == null)
            {
                return NotFound("Plan no encontrado.");
            }

            var newMemb = await _membershipService.RenewMembershipAsync(model.ClienteId, plan.Id, plan.DuracionDias, plan.Precio);
            await _paymentService.RegisterPaymentAsync(model.ClienteId, newMemb.Id, plan.Precio, model.MetodoPago);

            TempData["SuccessMessage"] = $"¡Membresía {plan.Nombre} renovada con éxito para {cliente.Nombre}!";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarEntrenador(int clienteId, int trainerId)
        {
            var success = await _staffService.AssignTrainerToClientAsync(clienteId, trainerId);
            if (!success)
            {
                TempData["ErrorMessage"] = "No se pudo realizar la asignación de entrenador.";
            }
            else
            {
                TempData["SuccessMessage"] = "¡Entrenador asignado con éxito y registrado en el historial!";
            }

            return RedirectToAction(nameof(Clientes));
        }
    }
}
