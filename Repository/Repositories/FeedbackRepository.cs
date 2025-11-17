using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(string status, Feedback? feedback)> CreateFeedbackAsync(Feedback feedback)
        {
            try
            {
                var result = await CreateAsync(feedback);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, feedback);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteFeedbackAsync(Guid id)
        {
            try
            {
                var existing = await GetByIdAsync(id);
                if (existing == null)
                    return "Post not found";

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

        public async Task<List<Feedback>> GetFeedbacksByCar(Guid carId)
        {
            return await _dbContext.Feedbacks
                .Where(x => x.CarId.Equals(carId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Feedback> UpdateFeedbackAsync(Feedback feedback)
        {
            try
            {
                var result = await UpdateAsync(feedback);

                return await GetByIdAsync(feedback.Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
