using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IInquiryImageRepository : IGenericRepository<InquiryImages>
    {
        Task AddInquiryImagesAsync(InquiryImages obj);
        Task<(string status, InquiryImages? InquiryImages)> CreateInquiryImagesAsync(InquiryImages inquiryImages);
    }
}
