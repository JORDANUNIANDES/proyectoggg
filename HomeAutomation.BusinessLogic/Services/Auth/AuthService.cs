using HomeAutomation.DataAccess.Entities;
using HomeAutomation.DataAccess.Repositories;
using System; // Para ArgumentNullException
using System.Threading.Tasks;
// Necesitarás una clase o librería para el hashing de contraseñas
// Ejemplo: using System.Security.Cryptography; o una librería como BCrypt.Net
// Para este ejemplo, usaremos un placeholder.
// using BCrypt.Net; // Ejemplo de lo que podrías usar

namespace HomeAutomation.BusinessLogic.Services.Auth
{
    // Interfaz placeholder para un servicio de hashing
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    }

    // Implementación placeholder MUY BÁSICA - NO USAR EN PRODUCCIÓN
    public class SimplePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            // ¡¡¡NO USAR ESTO EN UN PROYECTO REAL!!!
            // Considerar BCrypt.Net, PBKDF2, Argon2
            // Ejemplo: return BCrypt.HashPassword(password);
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                // En una implementación real, el salt debe ser único por usuario y almacenado con el hash.
                var salt = "SOME_STATIC_SALT_BAD_PRACTICE"; // ESTO ES INCORRECTO PARA PRODUCCIÓN
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + salt));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword)) return false;
            return hashedPassword == HashPassword(providedPassword); // Re-hashear y comparar (típico si el salt es estático o conocido)
                                                                    // Librerías como BCrypt tienen su propio método Verify.
                                                                    // Ejemplo: return BCrypt.Verify(providedPassword, hashedPassword);
        }
    }


    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null)
                return null;

            if (!_passwordHasher.VerifyHashedPassword(user.PasswordHash, password))
               return null;

            return user;
        }

        public async Task<string> RegisterUserAsync(string username, string password, UserRole role = UserRole.User)
        {
            if (string.IsNullOrWhiteSpace(username)) return "El nombre de usuario no puede estar vacío.";
            if (username.Length < 3) return "El nombre de usuario debe tener al menos 3 caracteres.";
            if (string.IsNullOrWhiteSpace(password)) return "La contraseña no puede estar vacía.";
            if (password.Length < 6) return "La contraseña debe tener al menos 6 caracteres.";
            // Aquí podrías añadir validaciones de fortaleza de contraseña más robustas.

            var existingUser = await _userRepository.GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                return "El nombre de usuario ya existe.";
            }

            var user = new User
            {
                Username = username,
                PasswordHash = _passwordHasher.HashPassword(password),
                Role = role
            };

            try
            {
                await _userRepository.AddUserAsync(user);
                return null; // Éxito
            }
            catch (Exception ex)
            {
                // Log ex (necesitaría ILogger inyectado)
                Console.WriteLine($"Error en RegisterUserAsync: {ex.Message}"); // Placeholder logging
                return "Ocurrió un error durante el registro.";
            }
        }
    }
}
