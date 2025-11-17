using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IFeedbackImageRepository : IGenericRepository<FeedbackImage>
    {
        Task<(string status, FeedbackImage? fbImage)> CreateFeedbackImageAsync(FeedbackImage fbImage);
    }
}
