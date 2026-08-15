using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MuscleHouse.Data;
using MuscleHouse.Models;
using MuscleHouse.ViewModels;

namespace MuscleHouse.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Role-based redirection
                    if (await _userManager.IsInRoleAsync(user, "Administrador"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Recepcionista"))
                    {
                        return RedirectToAction("Dashboard", "Recepcionista");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Entrenador"))
                    {
                        return RedirectToAction("Dashboard", "Entrenador");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Usuario"))
                    {
                        return RedirectToAction("Dashboard", "Cliente");
                    }
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido. Verifique sus credenciales.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "El correo electrónico ya está registrado.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Public registrations are STRICTLY assigned the "Usuario" role
            await _userManager.AddToRoleAsync(user, "Usuario");

            var cliente = new Cliente
            {
                UserId = user.Id,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Telefono = model.Telefono,
                FechaNacimiento = model.FechaNacimiento,
                Objetivo = model.Objetivo,
                Activo = true
            };

            _dbContext.Clientes.Add(cliente);

            var notif = new Notificacion
            {
                UserId = user.Id,
                Titulo = "¡Bienvenido a MUSCLE HOUSE!",
                Mensaje = "Tu registro se ha completado con éxito. Acércate a recepción cuando desees adquirir una membresía.",
                Fecha = DateTime.Now,
                Leida = false
            };
            _dbContext.Notificaciones.Add(notif);

            await _dbContext.SaveChangesAsync();

            // Auto sign-in user upon successful registration and redirect to Cliente Dashboard
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["SuccessMessage"] = "¡Tu cuenta ha sido creada exitosamente! Bienvenido a MUSCLE HOUSE.";
            return RedirectToAction("Dashboard", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
