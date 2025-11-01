using Repository.Base;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UnitOfWork _unitOfWork;
        private IJWTService _jwtService;
        public UserService(UnitOfWork unitOfWork, IJWTService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }
        public async Task<LoginResponse> AuthenticateAsync(string email, string password)
        {
            var user = await _unitOfWork._userRepo.Authentication(email, password);
            if (user != null)
            {
                return _jwtService.GenerateToken(user);
            }
            return null!;
        }

        public async Task<LoginResponse?> RegisterCustomer(RegisterRequest request)
        {
            var knownUser = await _unitOfWork._userRepo.GetFirstWithIncludeAsync(u => u.Email == request.Email);
            if (knownUser != null)
            {
                throw new Exception("Email already in use");
            }
            User newUser = new User()
            {
                Username = request.Username,
                Password = request.Password,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Fullname = request.Fullname,
                Address = request.Address,
                Gender = request.Gender,
                RoleId = 1, //Customer role
                IsGoogle = false,
                IsCarOwner = false,
                Status = "Pending",
            };
            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork._userRepo.CreateAsync(newUser);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                _unitOfWork.RollbackTransaction();
                return null;
            }
            User newlyCtUser = _unitOfWork._userRepo.GetByEmail(newUser.Email);
            return _jwtService.GenerateToken(newlyCtUser);
        }
    }
}
