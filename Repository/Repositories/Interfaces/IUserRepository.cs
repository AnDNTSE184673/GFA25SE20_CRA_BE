using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> LoginByGoogle(string email, string? name, string googleId);
        Task<(string status, User? user)> RegisterByGoogle(User user);
        Task<User?> Authentication(string email, string password);
        Task<List<User>> GetAllUserAsync();
        User? GetByEmail(string email);
        Task<User?> GetUserByUsernameAsync(string managerName);
        Task<User?> GetUserWithTokenAsync(Guid id);
    }
}
