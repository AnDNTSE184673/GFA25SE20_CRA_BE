using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Feedback;
using Repository.DTO.ResponseDTO.Feedbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IFeedbackService
    {
        Task<List<FeedbackView>> GetCarFeedbacks(Guid carId);
        Task<(string status, FeedbackView view)> LeaveCarFeedback(CreateFeedbackForm form);
        Task<FeedbackView> EditCarFeedback(Guid id, EditFeedbackForm form);
        Task<string> DeleteCarFeedback(Guid id);
    }
}
