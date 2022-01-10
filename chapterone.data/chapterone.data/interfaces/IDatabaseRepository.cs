using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace chapterone.data.interfaces
{
    public interface IDatabaseRepository<T>
        where T : IEntity
    {
        Task InsertAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(T item);
        Task DeleteAsync(string id);
        Task<T> GetByIdAsync(string id);
        Task<bool> ContainsAsync(string id);
        Task<long> CountAsync(Expression<Func<T, bool>> predicate);
        Task<IList<T>> QueryAsync(Expression<Func<T, bool>> predicate, int pageNumber = 0, int pageSize = 0);
        Task<IList<T>> QueryAsync<K>(Expression<Func<T, bool>> predicate, Expression<Func<T, K>> keySelector, bool isAscending = true, int pageNumber = 0, int pageSize = 0);
        IQueryable<T> AsQueryable();
    }
}
