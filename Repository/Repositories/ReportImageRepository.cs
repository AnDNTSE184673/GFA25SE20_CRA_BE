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
    public class ReportImageRepository : GenericRepository<ReportImage>, IReportImageRepository
    {
        public ReportImageRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task AddReportImagesAsync(ReportImage obj)
        {
            try
            {
                var result = await _dbContext.ReportImages.AddAsync(obj);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, ReportImage? reportImage)> CreateReportImagesAsync(ReportImage reportImage)
        {
            try
            {
                var result = await CreateAsync(reportImage);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, reportImage);
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
