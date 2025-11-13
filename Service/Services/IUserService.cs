using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO;
using Repository.DTO.ResponseDTO.User;
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
        Task<UserView> GetUser();
        Task<List<UserView>> GetAllUser();
    }
}
