using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EventWay.Query
{
    public interface IQueryModelRepository
    {
        Task<T> GetById<T>(Guid id) where T : QueryModel;

        Task<IEnumerable<T>> GetAll<T>() where T : QueryModel;
        Task<IEnumerable<T>> GetAll<T>(Expression<Func<T, bool>> predicate) where T : QueryModel;

        Task<T> QueryItemAsync<T>(Expression<Func<T, bool>> predicate) where T : QueryModel;

        Task Save(QueryModel queryModel);

        /*
        Task CreateItemAsync<T>(T item) where T : class;
        Task<T> GetItemAsync<T>(string id) where T : class;
        Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity;
        Task<T> QueryItemAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity;
        Task UpdateItemAsync<T>(string id, T item) where T : class;
        Task DeleteItemAsync<T>(string id) where T : class;*/
    }
}
