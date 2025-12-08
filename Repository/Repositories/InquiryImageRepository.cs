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
using static System.Net.Mime.MediaTypeNames;

namespace Repository.Repositories
{
    public class InquiryImageRepository : GenericRepository<InquiryImages>, IInquiryImageRepository
    {
        public InquiryImageRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task AddInquiryImagesAsync(InquiryImages obj)
        {
            try
            {
                var result = await _dbContext.InquiryImages.AddAsync(obj);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, InquiryImages? InquiryImages)> CreateInquiryImagesAsync(InquiryImages InquiryImages)
        {
            try
            {
                var result = await CreateAsync(InquiryImages);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, InquiryImages);
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
