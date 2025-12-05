using Repository.DTO.RequestDTO.Inquiry;
using Repository.DTO.ResponseDTO.Inquiry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IInquiryService
    {
        Task<List<InquiryView>> GetInquiriesByReceiver(Guid receiverId);
        Task<List<InquiryView>> GetInquiriesBySender(Guid senderId);
        Task<(string status, InquiryView view)> LeaveCarInquiry(CreateInquiryForm form);
        Task<(string status, InquiryView view)> AnswerCarInquiry(CreateInquiryForm form);
        Task<InquiryView> EditCarInquiry(Guid id, EditInquiryForm form);
        Task<string> DeleteCarInquiry(Guid id);
    }
}
