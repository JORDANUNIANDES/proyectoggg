using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuscleHouse.Data;
using MuscleHouse.Models;
using MuscleHouse.Services;
using MuscleHouse.ViewModels;

namespace MuscleHouse.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly IMembershipService _membershipService;
        private readonly IPaymentService _paymentService;
        private readonly IAttendanceService _attendanceService;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            IStaffService staffService,
            IMembershipService membershipService,
            IPaymentService paymentService,
            IAttendanceService attendanceService,
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _staffService = staffService;
            _membershipService = membershipService;
            _paymentService = paymentService;
            _attendanceService = attendanceService;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var trainers = await _staffService.GetAllTrainersAsync();
            var receptionists = await _staffService.GetAllReceptionistsAsync();
            var payments = await _paymentService.GetAllPaymentsAsync();
            var attendance = await _attendanceService.GetAllAttendanceAsync();

            decimal totalIncome = payments.Sum(p => p.Monto);

            var model = new AdminDashboardViewModel
            {
                TotalClientes = trainers.Sum(t => t.Clientes.Count),
                TotalEntrenadores = trainers.Count(),
                TotalRecepcionistas = receptionists.Count(),
                TotalMembresiasActivas = _dbContext.Membresias.Count(m => m.Estado == "Activa"),
                TotalIngresos = totalIncome,
                PagosRecientes = payments.Take(10).ToList(),
                AsistenciasRecientes = attendance.Take(10).ToList(),
                VentasPorPlan = payments
                    .Where(p => p.Membresia?.Plan != null)
                    .GroupBy(p => p.Membresia!.Plan!.Nombre)
                    .ToDictionary(g => g.Key, g => g.Count()),
                IngresosPorMetodoPago = payments
                    .GroupBy(p => p.MetodoPago)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Monto))
            };

            return View(model);
        }

        // ================= CLIENTES =================
        [HttpGet]
        public async Task<IActionResult> Clientes()
        {
            var clients = await _dbContext.Clientes.Include(c => c.User).Include(c => c.Entrenador).ToListAsync();
            return View(clients);
        }

        [HttpGet]
        public async Task<IActionResult> EditarCliente(int id)
        {
            var cliente = await _dbContext.Clientes.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null) return NotFound();

            ViewBag.Trainers = await _staffService.GetAllTrainersAsync();
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCliente(Cliente model)
        {
            var cliente = await _dbContext.Clientes.FindAsync(model.Id);
            if (cliente == null) return NotFound();

            cliente.Nombre = model.Nombre;
            cliente.Apellido = model.Apellido;
            cliente.Telefono = model.Telefono;
            cliente.FechaNacimiento = model.FechaNacimiento;
            cliente.Objetivo = model.Objetivo;
            cliente.EntrenadorId = model.EntrenadorId;

            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cliente editado con éxito.";
            return RedirectToAction(nameof(Clientes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InactivarCliente(int id)
        {
            var cliente = await _dbContext.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            cliente.Activo = !cliente.Activo;
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = cliente.Activo ? "Cliente activado con éxito." : "Cliente inactivado con éxito.";
            return RedirectToAction(nameof(Clientes));
        }

        // ================= ENTRENADORES =================
        [HttpGet]
        public async Task<IActionResult> Entrenadores()
        {
            var trainers = await _dbContext.Entrenadores.Include(e => e.User).ToListAsync();
            return View(trainers);
        }

        [HttpGet]
        public IActionResult CrearEntrenador()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEntrenador(string email, string password, string nombre, string apellido, string especialidad, int experiencia, string descripcion, IFormFile? fotografia)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Correo y contraseña son obligatorios.");
                return View();
            }

            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
                return View();
            }

            await _userManager.AddToRoleAsync(user, "Entrenador");

            string relativePath = "uploads/default-avatar.png";
            if (fotografia != null && fotografia.Length > 0)
            {
                var validationError = ValidateImage(fotografia);
                if (validationError != null)
                {
                    ModelState.AddModelError(string.Empty, validationError);
                    await _userManager.DeleteAsync(user);
                    return View();
                }

                relativePath = await SaveImageAsync(fotografia);
            }

            var trainer = new Entrenador
            {
                UserId = user.Id,
                Nombre = nombre,
                Apellido = apellido,
                Especialidad = especialidad,
                Experiencia = experiencia,
                Descripcion = descripcion,
                Fotografia = relativePath,
                Activo = true
            };

            _dbContext.Entrenadores.Add(trainer);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "¡Entrenador creado con éxito!";
            return RedirectToAction(nameof(Entrenadores));
        }

        [HttpGet]
        public async Task<IActionResult> EditarEntrenador(int id)
        {
            var trainer = await _dbContext.Entrenadores.FindAsync(id);
            if (trainer == null) return NotFound();
            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEntrenador(Entrenador model, IFormFile? nuevaFotografia)
        {
            var trainer = await _dbContext.Entrenadores.FindAsync(model.Id);
            if (trainer == null) return NotFound();

            trainer.Nombre = model.Nombre;
            trainer.Apellido = model.Apellido;
            trainer.Especialidad = model.Especialidad;
            trainer.Experiencia = model.Experiencia;
            trainer.Descripcion = model.Descripcion;

            if (nuevaFotografia != null && nuevaFotografia.Length > 0)
            {
                var validationError = ValidateImage(nuevaFotografia);
                if (validationError != null)
                {
                    ModelState.AddModelError(string.Empty, validationError);
                    return View(model);
                }
                trainer.Fotografia = await SaveImageAsync(nuevaFotografia);
            }

            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Entrenador editado con éxito.";
            return RedirectToAction(nameof(Entrenadores));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InactivarEntrenador(int id)
        {
            var trainer = await _dbContext.Entrenadores.FindAsync(id);
            if (trainer == null) return NotFound();

            trainer.Activo = !trainer.Activo;
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = trainer.Activo ? "Entrenador activado con éxito." : "Entrenador inactivado con éxito.";
            return RedirectToAction(nameof(Entrenadores));
        }

        // ================= RECEPCIONISTAS =================
        [HttpGet]
        public async Task<IActionResult> Recepcionistas()
        {
            var receptionists = await _dbContext.Recepcionistas.Include(r => r.User).ToListAsync();
            return View(receptionists);
        }

        [HttpGet]
        public IActionResult CrearRecepcionista()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearRecepcionista(string email, string password, string nombre, string apellido, string descripcion, string horarioAtencion, IFormFile? fotografia)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Correo y contraseña son obligatorios.");
                return View();
            }

            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
                return View();
            }

            await _userManager.AddToRoleAsync(user, "Recepcionista");

            string relativePath = "uploads/default-avatar.png";
            if (fotografia != null && fotografia.Length > 0)
            {
                var validationError = ValidateImage(fotografia);
                if (validationError != null)
                {
                    ModelState.AddModelError(string.Empty, validationError);
                    await _userManager.DeleteAsync(user);
                    return View();
                }

                relativePath = await SaveImageAsync(fotografia);
            }

            var recep = new Recepcionista
            {
                UserId = user.Id,
                Nombre = nombre,
                Apellido = apellido,
                Descripcion = descripcion,
                HorarioAtencion = horarioAtencion,
                Fotografia = relativePath,
                Activo = true
            };

            _dbContext.Recepcionistas.Add(recep);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "¡Recepcionista creado con éxito!";
            return RedirectToAction(nameof(Recepcionistas));
        }

        [HttpGet]
        public async Task<IActionResult> EditarRecepcionista(int id)
        {
            var recep = await _dbContext.Recepcionistas.FindAsync(id);
            if (recep == null) return NotFound();
            return View(recep);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecepcionista(Recepcionista model, IFormFile? nuevaFotografia)
        {
            var recep = await _dbContext.Recepcionistas.FindAsync(model.Id);
            if (recep == null) return NotFound();

            recep.Nombre = model.Nombre;
            recep.Apellido = model.Apellido;
            recep.HorarioAtencion = model.HorarioAtencion;
            recep.Descripcion = model.Descripcion;

            if (nuevaFotografia != null && nuevaFotografia.Length > 0)
            {
                var validationError = ValidateImage(nuevaFotografia);
                if (validationError != null)
                {
                    ModelState.AddModelError(string.Empty, validationError);
                    return View(model);
                }
                recep.Fotografia = await SaveImageAsync(nuevaFotografia);
            }

            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Recepcionista editado con éxito.";
            return RedirectToAction(nameof(Recepcionistas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InactivarRecepcionista(int id)
        {
            var recep = await _dbContext.Recepcionistas.FindAsync(id);
            if (recep == null) return NotFound();

            recep.Activo = !recep.Activo;
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = recep.Activo ? "Recepcionista activado con éxito." : "Recepcionista inactivado con éxito.";
            return RedirectToAction(nameof(Recepcionistas));
        }

        // ================= PLANES =================
        [HttpGet]
        public async Task<IActionResult> Planes()
        {
            var planes = await _dbContext.Planes.ToListAsync();
            return View(planes);
        }

        [HttpGet]
        public IActionResult CrearPlan()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPlan(Plan plan)
        {
            if (!ModelState.IsValid)
            {
                return View(plan);
            }

            if (plan.Precio <= 0 || plan.DuracionDias <= 0)
            {
                ModelState.AddModelError(string.Empty, "El precio y la duración deben ser mayores a cero.");
                return View(plan);
            }

            _dbContext.Planes.Add(plan);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "¡Plan de entrenamiento creado con éxito!";
            return RedirectToAction(nameof(Planes));
        }

        [HttpGet]
        public async Task<IActionResult> EditarPlan(int id)
        {
            var plan = await _dbContext.Planes.FindAsync(id);
            if (plan == null) return NotFound();
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPlan(Plan model)
        {
            if (!ModelState.IsValid) return View(model);

            var plan = await _dbContext.Planes.FindAsync(model.Id);
            if (plan == null) return NotFound();

            plan.Nombre = model.Nombre;
            plan.DuracionDias = model.DuracionDias;
            plan.Precio = model.Precio;
            plan.Descripcion = model.Descripcion;

            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Plan editado con éxito.";
            return RedirectToAction(nameof(Planes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InactivarPlan(int id)
        {
            var plan = await _dbContext.Planes.FindAsync(id);
            if (plan == null) return NotFound();

            plan.Activo = !plan.Activo;
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = plan.Activo ? "Plan activado con éxito." : "Plan inactivado con éxito.";
            return RedirectToAction(nameof(Planes));
        }

        // ================= MEMBRESÍAS, PAGOS Y ASISTENCIAS =================
        [HttpGet]
        public async Task<IActionResult> Membresias()
        {
            var membresias = await _dbContext.Membresias
                .Include(m => m.Cliente)
                .Include(m => m.Plan)
                .OrderByDescending(m => m.FechaInicio)
                .ToListAsync();
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
            return View(asistencias);
        }

        // ================= EJERCICIOS =================
        [HttpGet]
        public async Task<IActionResult> Ejercicios()
        {
            var ejercicios = await _dbContext.Ejercicios.ToListAsync();
            return View(ejercicios);
        }

        [HttpGet]
        public IActionResult CrearEjercicio()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEjercicio(Ejercicio model)
        {
            if (!ModelState.IsValid) return View(model);

            _dbContext.Ejercicios.Add(model);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ejercicio creado con éxito.";
            return RedirectToAction(nameof(Ejercicios));
        }

        [HttpGet]
        public async Task<IActionResult> EditarEjercicio(int id)
        {
            var ej = await _dbContext.Ejercicios.FindAsync(id);
            if (ej == null) return NotFound();
            return View(ej);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEjercicio(Ejercicio model)
        {
            if (!ModelState.IsValid) return View(model);

            var ej = await _dbContext.Ejercicios.FindAsync(model.Id);
            if (ej == null) return NotFound();

            ej.Nombre = model.Nombre;
            ej.GrupoMuscular = model.GrupoMuscular;
            ej.Descripcion = model.Descripcion;
            ej.Instrucciones = model.Instrucciones;

            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ejercicio editado con éxito.";
            return RedirectToAction(nameof(Ejercicios));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InactivarEjercicio(int id)
        {
            var ej = await _dbContext.Ejercicios.FindAsync(id);
            if (ej == null) return NotFound();

            ej.Activo = !ej.Activo;
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = ej.Activo ? "Ejercicio activado con éxito." : "Ejercicio inactivado con éxito.";
            return RedirectToAction(nameof(Ejercicios));
        }

        // ================= HELPER METHODS FOR IMAGES =================
        private string? ValidateImage(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return "La extensión de la imagen no está permitida. Use JPG, JPEG, PNG o WebP.";
            }

            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
            {
                return "El tipo MIME del archivo no está permitido.";
            }

            if (file.Length > 2 * 1024 * 1024)
            {
                return "La imagen excede el límite de tamaño permitido de 2 MB.";
            }

            return null;
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "uploads/" + uniqueFileName;
        }
    }
}
