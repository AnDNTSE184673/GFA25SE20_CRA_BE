using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Constant;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly CRA_DbContext _context;
        public UserRepository(CRA_DbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> LoginByGoogle(string email, string? name, string googleId)
        {

            return await _dbContext.Users
                .Where(x => (x.Email.Equals(email) || x.Username.Equals(name))
                && x.GoogleId == googleId
                && x.IsGoogle == true)
                .Include(x => x.Role)
                .FirstOrDefaultAsync();
        }

        public async Task<(string status, User? user)> RegisterByGoogle(User user)
        {
            try
            {
                //exist check is in services layer instead

                user.RoleId = (int)ConstantEnum.RoleID.CUSTOMER;
                user.Status = ConstantEnum.Statuses.ACTIVE;

                user.Id = Guid.NewGuid();
                user.IsGoogle = true;

                var result = await CreateAsync(user);

                if (result > 0)
                    return (ConstantEnum.RepoStatus.SUCCESS, user);
                else
                    return (ConstantEnum.RepoStatus.FAILURE, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task<User?> Authentication(string email, string password)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            return user;
        }

        public async Task<List<User>> GetAllUserAsync()
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .ToListAsync();
        }

        public User? GetByEmail(string email)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == email);
            return user;
        }

        public async Task<User?> GetUserByUsernameAsync(string managerName)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .Where(u => u.Username.Equals(managerName))
                .FirstOrDefaultAsync();
        }
    }
}
