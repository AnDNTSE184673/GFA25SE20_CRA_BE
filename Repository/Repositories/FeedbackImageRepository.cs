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
    public class FeedbackImageRepository : GenericRepository<FeedbackImage>, IFeedbackImageRepository
    {
        public FeedbackImageRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(string status, FeedbackImage? fbImage)> CreateFeedbackImageAsync(FeedbackImage fbImage)
        {
            try
            {
                var result = await CreateAsync(fbImage);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, fbImage);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
