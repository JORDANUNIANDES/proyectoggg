using HomeAutomation.DataAccess.Entities;
using System.Threading.Tasks; // Asumiendo operaciones async

namespace HomeAutomation.DataAccess.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByIdAsync(int userId);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        // Otros métodos si son necesarios, ej: GetAllUsersAsync()
    }
}
