using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IFeedbackRepository : IGenericRepository<Feedback>
    {
        Task<List<Feedback>> GetFeedbacksByCar(Guid carId);
        Task<(string status, Feedback? feedback)> CreateFeedbackAsync(Feedback feedback);
        Task<string> DeleteFeedbackAsync(Guid id);
        Task<Feedback> UpdateFeedbackAsync(Feedback feedback);
    }
}
