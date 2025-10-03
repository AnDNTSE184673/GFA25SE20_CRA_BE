using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Base
{
    public interface IGenericRepository<T> where T : class
    {
        // ========== READ ==========
        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();

        T? GetById(int id);
        Task<T?> GetByIdAsync(int id);

        T? GetById(string code);
        Task<T?> GetByIdAsync(string code);

        T? GetById(Guid code);
        Task<T?> GetByIdAsync(Guid code);

        Task<IEnumerable<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includeProperties);
        Task<T?> GetByIdWithIncludeAsync<TKey>(TKey TId, string typeId, params Expression<Func<T, object>>[] includeProperties);
        Task<T?> GetFirstWithIncludeAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);

        // ========== CREATE ==========
        int Create(T entity);
        Task<int> CreateAsync(T entity);

        int Create(IEnumerable<T> entities);
        Task<int> CreateAsync(IEnumerable<T> entities);

        // ========== UPDATE ==========
        int Update(T entity);
        Task<int> UpdateAsync(T entity);
        Task<T> UpdateAsyncReturnItem(T entity);

        // ========== DELETE ==========
        bool Remove(T entity);
        Task<bool> RemoveAsync(T? entity);
    }

}
