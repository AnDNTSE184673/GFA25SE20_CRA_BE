using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IUserService
    {
        Task<LoginResponse> AuthenticateAsync(string email, string password);
        Task<LoginResponse?> RegisterCustomer(RegisterRequest request);
        Task<User> CreateOwner(RegisterOwnerRequest request);
        Task<User?> UpdateToCarOwner(Guid userId);
        Task<User?> UpdateUserInfo(UserUpdateRequest request);
        Task<List<User>> GetAllUsers();
        Task<User?> GetUserById(Guid userId);
    }
}
