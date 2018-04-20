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
        Task DeleteByIds<T>(List<Guid> aggregateIds) where T : QueryModel;
        Task Save<T>(List<T> queryModels) where T : QueryModel;
        Task<List<T>> GetByIds<T>(List<Guid> aggregateIds) where T : QueryModel;
        Task<PagedResult<T>> GetPagedList<T>(PagedQuery pagedQuery) where T : QueryModel;
        Task<PagedResult<T>> GetPagedList<T>(PagedQuery pagedQuery, Expression<Func<T, bool>> predicate) where T : QueryModel;
        Task<bool> DoesItemExist<T>(Guid id) where T : QueryModel;
        Task<bool> DoesItemExist<T>(Expression<Func<T, bool>> predicate) where T : QueryModel;
        Task<List<dynamic>> ExecuteRawSql(string sql);
        Task ClearCollectionAsync();

        /*
		Task SaveQueryModelList<T>(List<T> queryModels) where T : QueryModel;
		Task DeleteMultipleModel<T>(List<Guid> ids) where T : QueryModel;

		Task<PagedResult<T>> GetPagedListAsync<T>(PagedQuery pagedQuery) where T : QueryModel;
		Task<PagedResult<T>> GetPagedListAsync<T>(PagedQuery pagedQuery, Expression<Func<T, bool>> predicate) where T : QueryModel;
        IQueryable<T> CreatePagedListQuery<T>(PagedQuery pagedQuery, Expression<Func<T, bool>> predicate) where T : QueryModel;

        Task<int> QueryCountAsync<T>() where T : QueryModel;

		Task<bool> DoesItemExist<T>(Guid id) where T : QueryModel;

		Task<bool> DoesItemExist<T>(Expression<Func<T, bool>> predicate) where T : QueryModel;

		Task ClearCollectionAsync();

		Task<List<dynamic>> ExecuteRawSql(string sql);
		Task<List<T>> GetByIds<T>(List<Guid> ids) where T : QueryModel;
        */
    }
}
