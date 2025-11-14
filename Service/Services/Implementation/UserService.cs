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
    }
}
