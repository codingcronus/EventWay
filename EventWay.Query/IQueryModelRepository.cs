using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EventWay.Query
{
    public interface IQueryModelRepository
    {
        Task Save(QueryModel queryModel);
        
        Task<T> GetById<T>(Guid id) where T : QueryModel;

        Task<IEnumerable<T>> GetAll<T>() where T : QueryModel;
        Task<IEnumerable<T>> GetAll<T>(Expression<Func<T, bool>> predicate) where T : QueryModel;

        Task<T> QueryItemAsync<T>(Expression<Func<T, bool>> predicate) where T : QueryModel;

        Task DeleteById<T>(Guid id) where T : QueryModel;
    }
}
