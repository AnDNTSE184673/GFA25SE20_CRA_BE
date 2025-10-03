using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Base
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly CRA_DbContext _dbContext;
        public DbSet<T> Entities { get; }

        public GenericRepository(CRA_DbContext dbContext)
        {
            _dbContext = dbContext;
            Entities = _dbContext.Set<T>();
        }

        // ========== READ ==========
        public IEnumerable<T> GetAll()
        {
            return Entities.ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Entities.ToListAsync();
        }

        public T? GetById(int id)
        {
            var entity = Entities.Find(id);
            if (entity != null) _dbContext.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var entity = await Entities.FindAsync(id);
            if (entity != null) _dbContext.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public T? GetById(string code)
        {
            var entity = Entities.Find(code);
            if (entity != null) _dbContext.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async Task<T?> GetByIdAsync(string code)
        {
            var entity = await Entities.FindAsync(code);
            if (entity != null) _dbContext.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public T? GetById(Guid code)
        {
            var entity = Entities.Find(code);
            if (entity != null) _dbContext.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async Task<T?> GetByIdAsync(Guid code)
        {
            var entity = await Entities.FindAsync(code);
            if (entity != null) _dbContext.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        /// <summary>
        /// Get all with generic include of entity
        /// EXAMPLE HOW TO USE
        ///  var products = (await _unitOfWork.ProductRepository.GetAllWithIncludeAsync(p => p.Category)).AsQueryable();
        /// </summary>
        public async Task<IEnumerable<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = Entities;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty).AsNoTracking();
            }

            return await query.ToListAsync();

        }

        /// <summary>
        /// Get detail with id and with generic include of entity
        /// EXAMPLE HOW TO USE
        /// var foundCart = await _unitOfWork.CartRepository.GetByIdWithIncludeAsync(request.CartId, "CartId", cart => cart.ShoppingCartItems);
        /// </summary>
        public async Task<T?> GetByIdWithIncludeAsync<TKey>(TKey TId, string typeId, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = Entities;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty).AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(entity => EF.Property<TKey>(entity, typeId).Equals(TId));

        }

        /// <summary>
        /// Get the first element that matches the predicate
        /// and Include the objects that matches the expression array
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="includeProperties">Include expression array</param>
        /// <returns>A matching entity or null if not found"</returns>
        public async Task<T?> GetFirstWithIncludeAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = Entities;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        // ========== CREATE ==========
        public int Create(T entity)
        {
            Entities.Add(entity);
            return _dbContext.SaveChanges();
        }

        public async Task<int> CreateAsync(T entity)
        {
            Entities.Add(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public int Create(IEnumerable<T> entities)
        {
            Entities.AddRange(entities);
            return _dbContext.SaveChanges();
        }

        public async Task<int> CreateAsync(IEnumerable<T> entities)
        {
            Entities.AddRange(entities);
            return await _dbContext.SaveChangesAsync();
        }

        // ========== UPDATE ==========
        public int Update(T entity)
        {
            _dbContext.Attach(entity).State = EntityState.Modified;
            return _dbContext.SaveChanges();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            _dbContext.Attach(entity).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<T> UpdateAsyncReturnItem(T entity)
        {
            _dbContext.Attach(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // ========== DELETE ==========
        public bool Remove(T entity)
        {
            Entities.Remove(entity);
            _dbContext.SaveChanges();
            return true;
        }

        public async Task<bool> RemoveAsync(T? entity)
        {
            if (entity == null) return false;
            Entities.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
