using AutoMapper;
using Newtonsoft.Json.Linq;
using Repository.Base;
using Repository.CustomFunctions.TokenHandler;
﻿using AutoMapper;
using Repository.Base;
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
using Microsoft.IdentityModel.Tokens;
using Repository.Constant;
using Repositories.DTO.ResponseDTO.User;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.Extensions.Configuration;
using Services.Service;
using Microsoft.Extensions.Logging;

namespace Service.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UnitOfWork _unitOfWork;
        private JWTTokenProvider _jwtService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IEmailService _email;
        private readonly ILogger<UserService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<UserService>();

        public UserService(UnitOfWork unitOfWork, JWTTokenProvider jwtService, IMapper mapper, IConfiguration config, IEmailService email)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _mapper = mapper;
            _config = config;
            _email = email;
        }

        public async Task<(UserLoginView? login, UserPostRegView? register)> GoogleLogin(string email, string name, string googleId)
        {
            if (email.IsNullOrEmpty() && name.IsNullOrEmpty() && googleId.IsNullOrEmpty())
                return (null, null);
            //This is the existing check, check account with similar email or name AND googleId
            //If login failed = account doesnt exist, so register an account with google
            var existing = await _unitOfWork._userRepo.LoginByGoogle(email, name, googleId);
            if (existing == null)
            {
                var result = await GoogleRegister(email, name, googleId);
                return (null, result.user);
            }
            else
            {
                var token = _jwtService.GenerateAccessToken(existing);
                var refreshToken = _jwtService.GenerateRefreshToken();

                await RefreshTokenAsync(refreshToken, existing);

                var mapped = _mapper.Map<UserLoginView?>(existing); //include Role
                mapped.JwtToken = token.token;
                return (mapped, null);
            }
        }

        public async Task<(string status, UserPostRegView? user)> GoogleRegister(string email, string name, string googleId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                RegisterUserForm regUser = new RegisterUserForm();
                regUser.Email = email;
                regUser.Username = name;
                regUser.GoogleId = googleId;

                var regData = _mapper.Map<User>(regUser);

                var response = await _unitOfWork._userRepo.RegisterByGoogle(regData);

                await _unitOfWork.CommitTransactionAsync();
                if (response.status.Equals(ConstantEnum.RepoStatus.SUCCESS))
                {
                    var body = _email.GenerateBodyRegisterSuccess(response.user.Username, response.user.Password, "S", "");
                    _email.SendEmailAsync("YuuZone Account Registration", body, response.user.Email, response.user.Fullname);
                    return (response.status, _mapper.Map<UserPostRegView>(response.user));
                }
                else
                    return (response.status, null);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task RefreshTokenAsync(string refreshToken, User user)
        {
            var refreshExpiry = DateTime.UtcNow.AddDays(_config.GetValue<int>("JWT:RefreshTokenDay"));

            var update = await _unitOfWork._userRepo.UpdateAsync(user);
            if (update < 1)
                _logger.LogError("Something broke when updating refresh token in DB");
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

        public async Task<User> CreateOwner(RegisterOwnerRequest request)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var knownUser = _unitOfWork._userRepo.GetByEmail(request.Email);
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
                    ImageAvatar = request.ImageAvatar,
                    IsCarOwner = request.IsCarOwner,
                    Rating = request.Rating,
                    Status = request.Status,
                    RoleId = request.RoleId
                };
                await _unitOfWork._userRepo.CreateAsync(newUser);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                User newOnwer = _unitOfWork._userRepo.GetByEmail(newUser.Email);
                return newOnwer;
            }
            catch
            {
                _unitOfWork.RollbackTransaction();
                return null!;
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            return (List<User>)await _unitOfWork._userRepo.GetAllAsync();
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            return await _unitOfWork._userRepo.GetByIdAsync(userId);
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

        public async Task<User?> UpdateToCarOwner(Guid userId)
        {
            var user = _unitOfWork._userRepo.GetById(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            _unitOfWork.BeginTransaction();
            user.IsCarOwner = true;
            try
            {
                await _unitOfWork._userRepo.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return user;
            }
            catch
            {
                _unitOfWork.RollbackTransaction();
                return null;
            }
        }

        public async Task<User?> UpdateUserInfo(UserUpdateRequest request)
        {
            var user = await _unitOfWork._userRepo.GetByIdAsync(request.Id);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            _unitOfWork.BeginTransaction();
            try
            {
                user.Fullname = request.Fullname;
                user.Password = request.Password;
                user.PhoneNumber = request.PhoneNumber;
                user.Address = request.Address;
                user.ImageAvatar = request.ImageAvatar;
                user.Status = request.Status;
                user.Gender = request.Gender;
                user.Username = request.Username;
                await _unitOfWork._userRepo.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                var result = await _unitOfWork._userRepo.GetByIdAsync(request.Id);
                if (result != null)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return result;
                }
                else
                {
                    throw new Exception("Update failed");
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Update failed: " + ex.Message);
            }
        }

        public async Task<User?> GetUserWithToken(Guid userId)
        {
            return await _unitOfWork._userRepo.GetUserWithTokenAsync(userId);
        }
    }
}
