using Microsoft.EntityFrameworkCore;
using MimeKit;
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
using static Repository.Constant.ConstantEnum;

namespace Repository.Repositories
{
    public class InquiryRepository : GenericRepository<Inquiry>, IInquiryRepository
    {
        public InquiryRepository(CRA_DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(string status, Inquiry obj)> CreateInquiryAsync(Inquiry newInquiry)
        {
            try
            {
                var result = await CreateAsync(newInquiry);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, newInquiry);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteInquiryAsync(Guid id)
        {
            try
            {
                var existing = await GetByIdAsync(id);
                if (existing == null)
                    return "Post not found";

                await RemoveAsync(existing);

                if (await GetByIdAsync(id) == null)
                    return ConstantEnum.RepoStatus.SUCCESS;
                else
                    return ConstantEnum.RepoStatus.FAILURE;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Inquiry> GetRootInquiryByBothUser(Guid senderId, Guid receiverId)
        {
            return await _dbContext.Inquiries
                .Where(x => x.ParentInquiryId == null)
                .Where(x => 
                (x.ReceiverId.Equals(receiverId) && x.SenderId.Equals(senderId))
                || (x.SenderId.Equals(receiverId) && x.ReceiverId.Equals(senderId))
                ).FirstOrDefaultAsync();
        }

        public async Task<List<Inquiry>> GetInquiryTreeFromRoot(Guid rootInquiryId)
        {
            var result = await _dbContext.Inquiries
                .FromSqlInterpolated($@"
                    WITH RECURSIVE Thread AS (
                        SELECT * FROM ""Inquiries"" WHERE ""Id"" = {rootInquiryId}
                        UNION ALL
                        SELECT c.* FROM ""Inquiries"" c
                        INNER JOIN Thread p ON c.""ParentInquiryId"" = p.""Id""
                    )
                    SELECT * FROM Thread ORDER BY ""CreateDate""
                ").Include(x => x.InquiryImages).ToListAsync();
            return result;
        }

        public async Task<List<Inquiry>> GetUserConversations(Guid userId)
        {
            var convos = await _dbContext.Inquiries
                .Where(x => x.SenderId == userId || x.ReceiverId == userId)
                .Include(x => x.InquiryImages)
                .Select(x => new
                {
                    Inquiry = x, //original rows
                    //if user is sender, find the other and vice versa
                    Partner = x.SenderId.Equals(userId) ? x.Receiver : x.Sender, //.Include() the user info of both
                    PartnerId = x.SenderId.Equals(userId) ? x.ReceiverId : x.SenderId
                })
                .ToListAsync();

            var conversations = convos
                .GroupBy(x => x.PartnerId)
                .Select(g => g
                    .OrderByDescending(m => m.Inquiry.CreateDate)
                    .First().Inquiry)
                .OrderByDescending(x => x.CreateDate)
                .ToList();

            return conversations;
        }

        public async Task<List<Inquiry>> GetAllConversationsBetween2Users(Guid senderId, Guid receiverId)
        {
            var convos = await _dbContext.Inquiries
                .Where(x =>
                (x.ReceiverId.Equals(receiverId) && x.SenderId.Equals(senderId))
                || (x.SenderId.Equals(receiverId) && x.ReceiverId.Equals(senderId))
                )
                .Include(x => x.InquiryImages)
                .ToListAsync();
            return convos;
        }

            public async Task<Inquiry> UpdateInquiryAsync(Inquiry inquiry)
        {
            try
            {
                var result = await UpdateAsync(inquiry);

                _dbContext.ChangeTracker.Clear();

                return await GetByIdAsync(inquiry.Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
