using HomeAutomation.DataAccess.Entities;
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Services.Auth
{
    public interface IAuthService
    {
        Task<User> AuthenticateAsync(string username, string password);
        // bool Authorize(User user, UserRole requiredRole); // Podría ser síncrono si el user ya está cargado
        Task<string> RegisterUserAsync(string username, string password, UserRole role = UserRole.User); // Devuelve mensaje de error o null
    }
}
