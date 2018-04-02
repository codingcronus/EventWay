using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EventWay.Query;

namespace EventWay.SampleApp
{
    public interface IExtendedQueryModelRepository : IQueryModelRepository
    {
        Task<List<dynamic>> GetRawSql(string sql);

        Task<PagedResult<T>> GetPagedListAsync<T>(PagedQuery pagedQuery) where T : QueryModel;
        Task<PagedResult<T>> GetPagedListAsync<T>(PagedQuery pagedQuery, Expression<Func<T, bool>> predicate) where T : QueryModel;
    }
}