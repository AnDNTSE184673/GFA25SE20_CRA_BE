using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IReportImageRepository : IGenericRepository<ReportImage>
    {
        Task AddReportImagesAsync(ReportImage obj);
        Task<(string status, ReportImage? reportImage)> CreateReportImagesAsync(ReportImage reportImage);
    }
}
