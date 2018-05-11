using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EventWay.Query
{
	public interface IQueryModelRepository
	{
        Task Save(QueryModel queryModel);

        Task<T> GetById<T>(Guid aggregateId) where T : QueryModel;
        Task<IEnumerable<T>> GetAll<T>() where T : QueryModel;

        Task<T> QueryItem<T>(Expression<Func<T, bool>> predicate) where T : QueryModel;
        Task<IEnumerable<T>> QueryAll<T>(Expression<Func<T, bool>> predicate) where T : QueryModel;

        Task DeleteById<T>(Guid aggregateId) where T : QueryModel;
    }
}
