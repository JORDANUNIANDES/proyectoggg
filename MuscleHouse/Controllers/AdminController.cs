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
                    await _userManager.DeleteAsync(user); // rollback user
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

        // ================= HELPER METHODS FOR IMAGES =================
        private string? ValidateImage(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return "La extensión de la imagen no está permitida. Use JPG, JPEG, PNG o WebP.";
            }

            // Check mime type safely
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
            {
                return "El tipo MIME del archivo no está permitido.";
            }

            // Max size: 2 MB
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
