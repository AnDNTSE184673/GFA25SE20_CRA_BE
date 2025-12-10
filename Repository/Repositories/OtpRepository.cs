using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Constant;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class OtpRepository : GenericRepository<OTPCode>, IOtpRepository
    {
        public OtpRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(string status, OTPCode obj)> CreateOtpAsync(OTPCode otp)
        {
            try
            {
                var result = await CreateAsync(otp);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, otp);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteOtpAsync(Guid id)
        {
            try
            {
                var existing = await GetByIdAsync(id);
                if (existing == null)
                    return "OTP Code not found";

                await RemoveAsync(existing);

                if (await GetByIdAsync(id) == null)
                    return ConstantEnum.RepoStatus.SUCCESS;
                else
                    return ConstantEnum.RepoStatus.FAILURE;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<OTPCode> FindSentOtpByUserAsync(Guid userId)
        {
            return await _dbContext.OTPCodes
                .Where(x => x.UserId.Equals(userId) && x.IsUsed == false)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
