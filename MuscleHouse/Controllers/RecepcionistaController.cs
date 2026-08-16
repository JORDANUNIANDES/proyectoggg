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
        public async Task<IActionResult> Clientes(string? search, string? estado, int page = 1)
        {
            var query = _dbContext.Clientes
                .Include(c => c.User)
                .Include(c => c.Entrenador)
                .Include(c => c.Membresias)
                .ThenInclude(m => m.Plan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c => c.Nombre.ToLower().Contains(term) ||
                                         c.Apellido.ToLower().Contains(term) ||
                                         (c.Nombre + " " + c.Apellido).ToLower().Contains(term) ||
                                         c.Telefono.Contains(term) ||
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
        public async Task<IActionResult> CrearCliente()
        {
            ViewBag.Trainers = await _staffService.GetAllTrainersAsync();
            var activePlans = await _dbContext.Planes.Where(p => p.Activo).ToListAsync();
            ViewBag.Plans = activePlans;
            ViewBag.Planes = activePlans;
            return View(new CreateClienteViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCliente(CreateClienteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Trainers = await _staffService.GetAllTrainersAsync();
                var activePlans = await _dbContext.Planes.Where(p => p.Activo).ToListAsync();
                ViewBag.Plans = activePlans;
                ViewBag.Planes = activePlans;
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
                var activePlans = await _dbContext.Planes.Where(p => p.Activo).ToListAsync();
                ViewBag.Plans = activePlans;
                ViewBag.Planes = activePlans;
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
                if (plan != null && plan.Activo)
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
        public async Task<IActionResult> Membresias(string? search, string? estado, int page = 1)
        {
            var query = _dbContext.Membresias.Include(m => m.Cliente).Include(m => m.Plan).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(m => (m.Cliente != null && (m.Cliente.Nombre.ToLower().Contains(term) || m.Cliente.Apellido.ToLower().Contains(term) || (m.Cliente.Nombre + " " + m.Cliente.Apellido).ToLower().Contains(term))) ||
                                         (m.Plan != null && m.Plan.Nombre.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(m => m.Estado == estado);
            }

            int totalItems = await query.CountAsync();
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var membresias = await query.OrderByDescending(m => m.FechaInicio)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var activePlans = await _dbContext.Planes.Where(p => p.Activo).ToListAsync();
            ViewBag.Clients = await _dbContext.Clientes.Where(c => c.Activo).ToListAsync();
            ViewBag.Plans = activePlans;
            ViewBag.Planes = activePlans;
            ViewBag.Search = search;
            ViewBag.Estado = estado;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(membresias);
        }

        [HttpGet]
        public async Task<IActionResult> Pagos(string? search, string? metodo, int page = 1)
        {
            var query = _dbContext.Pagos.Include(p => p.Cliente).Include(p => p.Membresia).ThenInclude(m => m.Plan).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => (p.Cliente != null && (p.Cliente.Nombre.ToLower().Contains(term) || p.Cliente.Apellido.ToLower().Contains(term) || (p.Cliente.Nombre + " " + p.Cliente.Apellido).ToLower().Contains(term))) ||
                                         (p.Membresia != null && p.Membresia.Plan != null && p.Membresia.Plan.Nombre.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(metodo))
            {
                query = query.Where(p => p.MetodoPago == metodo);
            }

            int totalItems = await query.CountAsync();
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagos = await query.OrderByDescending(p => p.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Metodo = metodo;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(pagos);
        }

        [HttpGet]
        public async Task<IActionResult> Asistencias(string? search, int page = 1)
        {
            var query = _dbContext.Asistencias.Include(a => a.Cliente).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a => a.Cliente != null && (a.Cliente.Nombre.ToLower().Contains(term) || a.Cliente.Apellido.ToLower().Contains(term) || (a.Cliente.Nombre + " " + a.Cliente.Apellido).ToLower().Contains(term) || a.Cliente.Telefono.Contains(term)));
            }

            int totalItems = await query.CountAsync();
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var asistencias = await query.OrderByDescending(a => a.FechaHora)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Clientes = await _dbContext.Clientes.Where(c => c.Activo).ToListAsync();
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

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
                TempData["ErrorMessage"] = "Datos de renovación inválidos.";
                return RedirectToAction(nameof(Membresias));
            }

            var cliente = await _staffService.GetClienteByIdAsync(model.ClienteId);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            var plan = await _dbContext.Planes.FindAsync(model.PlanId);
            if (plan == null || !plan.Activo)
            {
                TempData["ErrorMessage"] = "El plan seleccionado no existe o se encuentra inactivo.";
                return RedirectToAction(nameof(Membresias));
            }

            var newMemb = await _membershipService.RenewMembershipAsync(model.ClienteId, plan.Id, plan.DuracionDias, plan.Precio);
            await _paymentService.RegisterPaymentAsync(model.ClienteId, newMemb.Id, plan.Precio, model.MetodoPago);

            TempData["SuccessMessage"] = $"¡Membresía {plan.Nombre} renovada con éxito para {cliente.Nombre}!";
            return RedirectToAction(nameof(Membresias));
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
