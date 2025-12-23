using Microsoft.AspNetCore.Http;
using Repositories.DTO.ResponseDTO.User;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.RequestDTO.User;
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
        Task<(UserLoginView? login, UserPostRegView? register)> GoogleLogin(string email, string name, string googleId);
        Task<(string status, UserPostRegView? user)> GoogleRegister(string email, string name, string googleId);
        Task<(string msg, LoginResponse token)> AuthenticateAsync(string email, string password);
        Task<LoginResponse?> RegistrationVerificationAsync(string OTPCode, string phoneNumber);
        Task RegisterCustomer(RegisterRequest request);
        Task<User> CreateOwner(RegisterOwnerRequest request);
        Task<User?> UpdateToCarOwner(Guid userId);
        Task<User?> UpdateUserInfo(UserUpdateRequest request);
        Task<string> UpdateUserPasswordAsync(UpdatePasswordRequest request);
        Task<string> AuthorizeUpdateUserPasswordAsync(string email, string OtpCode);
        Task<List<User>> GetAllUsers();
        Task<User?> GetUserById(Guid userId);
        Task<User?> GetUserWithToken(Guid userId);
        Task<UserView> UpdateUserAvatarAsync(IFormFile image, Guid userId);
        Task<UserView> ResetUserReputation(Guid userId);
        Task<string> UpdateUserPhoneNumber(UpdatePhoneNumberRequest request);
        Task<string> AuthorizeUpdateUserPhoneNumberAsync(string phoneNumber, string OtpCode);
    }
}
