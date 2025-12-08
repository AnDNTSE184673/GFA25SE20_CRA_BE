using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IInquiryRepository : IGenericRepository<Inquiry>
    {
        Task<(string status, Inquiry obj)> CreateInquiryAsync(Inquiry newInquiry);
        Task<Inquiry> GetRootInquiryByBothUser(Guid senderId, Guid receiverId);
        Task<List<Inquiry>> GetInquiryTreeFromRoot(Guid rootInquiryId);
        Task<List<Inquiry>> GetUserConversations(Guid userId);
        Task<Inquiry> UpdateInquiryAsync(Inquiry inquiry);
        Task<List<Inquiry>> GetAllConversationsBetween2Users(Guid senderId, Guid receiverId);
    }
}
