using AutoMapper;
using Newtonsoft.Json.Linq;
using Repository.Base;
using Repository.CustomFunctions.TokenHandler;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO;
using Repository.DTO.ResponseDTO.User;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UnitOfWork _unitOfWork;
        private JWTTokenProvider _jwtService;
        private readonly IMapper _mapper;

        public UserService(UnitOfWork unitOfWork, JWTTokenProvider jwtService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public async Task<LoginResponse> AuthenticateAsync(string email, string password)
        {
            var user = await _unitOfWork._userRepo.Authentication(email, password);
            if (user != null)
            {
                var result = _jwtService.GenerateAccessToken(user);
                return new LoginResponse
                {
                    Token = result.token,
                    Expiration = result.expire
                };
            }
            return null!;
        }

        public async Task<List<UserView>> GetAllUser()
        {
            var result = await _unitOfWork._userRepo.GetAllUserAsync();
            return _mapper.Map<List<UserView>>(result);
        }

        public Task<UserView> GetUser()
        {
            throw new NotImplementedException();
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
            var result = _jwtService.GenerateAccessToken(newlyCtUser);
            return new LoginResponse
            {
                Token = result.token,
                Expiration = result.expire
            };
        }

    }
}
