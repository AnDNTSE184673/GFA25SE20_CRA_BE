using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.ResponseDTO;
using Supabase.Gotrue.Mfa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class OTPService : IOTPService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly IEmailService _email;

        public OTPService(IMapper mapper, UnitOfWork unitOfWork, IEmailService email)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _email = email;
        }

        /// <summary>
        /// This creates an OtpCode entry to store in the DB, then return a string of the unhashed code to attach to email or message
        /// </summary>
        public async Task<string> SendOTPCodes(Guid userId, string? additionalInfo)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var exist = await _unitOfWork._userRepo.GetByIdAsync(userId);
                if (exist == null) throw new KeyNotFoundException("User doesn't exist");

                var code = GenerateOtp();

                var newOtp = new OTPCode
                {
                    UserId = exist.Id,
                    OtpHash = BCrypt.Net.BCrypt.HashPassword(code),
                    ExpirationTime = DateTime.UtcNow.AddMinutes(10),
                    CreatedAt = DateTime.UtcNow,
                    AdditionalInfo = !additionalInfo.IsNullOrEmpty() ? additionalInfo : null
                };

                var result = await _unitOfWork._OtpRepo.CreateAsync(newOtp);

                await _unitOfWork.CommitTransactionAsync();

                if (result.Equals(ConstantEnum.RepoStatus.FAILURE))
                    return null;
                else
                    return code;
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Invalidate an existing active OTP code if there is one, then regenerate and send back a new one, also return an unhashed string code
        /// </summary>
        public async Task<string> ResendOTPCodes(Guid userId, string? additionalInfo)
        {
            try
            {
                var exist = await _unitOfWork._userRepo.GetByIdAsync(userId);
                if (exist == null) throw new KeyNotFoundException("User doesn't exist");
                var OtpSent = await _unitOfWork._OtpRepo.FindSentOtpByUserAsync(userId);
                if(OtpSent != null)
                {
                    OtpSent.IsUsed = true;

                    await _unitOfWork.BeginTransactionAsync();
                    await _unitOfWork._OtpRepo.UpdateAsync(OtpSent);
                    await _unitOfWork.CommitTransactionAsync();
                }

                var code = GenerateOtp();

                var newOtp = new OTPCode
                {
                    UserId = exist.Id,
                    OtpHash = BCrypt.Net.BCrypt.HashPassword(code),
                    ExpirationTime = DateTime.UtcNow.AddMinutes(10),
                    CreatedAt = DateTime.UtcNow,
                    AdditionalInfo = additionalInfo.IsNullOrEmpty() ? additionalInfo : null
                };

                var result = await _unitOfWork._OtpRepo.CreateAsync(newOtp);

                await _unitOfWork.CommitTransactionAsync();

                if (result.Equals(ConstantEnum.RepoStatus.FAILURE))
                    return null;
                else
                    return code;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Don't actually call this, call OTPVerificationAsync() instead
        /// </summary>
        public async Task<(OTPCode entry, string message)> SubmitOTPCodes(Guid userId, string unhashedCode)
        {
            try
            {
                var OtpSent = await _unitOfWork._OtpRepo.FindSentOtpByUserAsync(userId);
                if (OtpSent != null && OtpSent.AttemptCount <= 3 && OtpSent.ExpirationTime > DateTime.UtcNow)
                {
                    bool verify = BCrypt.Net.BCrypt.Verify(unhashedCode, OtpSent.OtpHash);
                    if (!verify)
                        return (OtpSent, ConstantEnum.Statuses.DENIED);
                    return (OtpSent, ConstantEnum.Statuses.APPROVED);
                }
                else if (OtpSent == null)
                {
                    throw new KeyNotFoundException("This user has no verification code sent");
                }
                else if (OtpSent.AttemptCount > 3)
                {
                    throw new ArgumentException("This code has no tries left!");
                }
                else
                {
                    throw new ArgumentException("This code has expired!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Exposes the SubmitOTPCodes function
        /// </summary>
        public async Task<(OTPCode entry, string message)> OTPVerificationAsync(string OTPCode, Guid userId)
        {
            try
            {
                var user = _unitOfWork._userRepo.GetById(userId);
                if (user == null) throw new KeyNotFoundException("User not found!");
                var result = await SubmitOTPCodes(user.Id, OTPCode);
                var otpStatus = result.entry;
                if (result.message.Equals(ConstantEnum.Statuses.APPROVED))
                {

                    otpStatus.IsUsed = true;

                    await _unitOfWork.BeginTransactionAsync();
                    await _unitOfWork._OtpRepo.UpdateAsync(otpStatus);
                    await _unitOfWork.CommitTransactionAsync();

                    return (otpStatus, ConstantEnum.RepoStatus.SUCCESS);
                }
                else
                {
                    otpStatus.AttemptCount++;

                    await _unitOfWork.BeginTransactionAsync();
                    await _unitOfWork._OtpRepo.UpdateAsync(otpStatus);
                    await _unitOfWork.CommitTransactionAsync();

                    return (otpStatus, ConstantEnum.RepoStatus.FAILURE);
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public static string GenerateOtp(int length = 6)
        {
            // digits 0–9
            const string digits = "0123456789";

            var bytes = RandomNumberGenerator.GetBytes(length);
            char[] otp = new char[length];

            for (int i = 0; i < length; i++)
            {
                // map random byte to [0..9]
                otp[i] = digits[bytes[i] % digits.Length];
            }

            return new string(otp);
        }
    }
}
