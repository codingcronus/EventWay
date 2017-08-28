using EventWay.Query;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EventWay.Infrastructure.CosmosDb
{
    public class DocumentDbQueryModelRepository : IQueryModelRepository
    {
        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly string _endpoint;
        private readonly string _authKey;
        //private readonly IReliableReadWriteDocumentClient _client;
        private DocumentClient _client;

        public DocumentDbQueryModelRepository(string database, string collection, string endpoint, string authKey)
        {
            _databaseId = database;
            _collectionId = collection;
            _endpoint = endpoint;
            _authKey = authKey;

            _client = new DocumentClient(new Uri(_endpoint), _authKey,
               new ConnectionPolicy
               {
                   EnableEndpointDiscovery = false
               }
            );
            //.AsReliable(new FixedInterval(10, TimeSpan.FromSeconds(1)));
        }

        public void Initialize()
        {
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        public async Task DeleteById<T>(Guid id) where T : QueryModel
        {
            try
            {
                var modelId = typeof(T).Name + "-" + id;

                var response = await _client.DeleteDocumentAsync(GetDocumentUri(modelId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return;

                throw;
            }
        }

        public async Task<T> GetById<T>(Guid id) where T : QueryModel
        {
            try
            {
                var modelId = typeof(T).Name + "-" + id;

                Document document = await _client.ReadDocumentAsync(GetDocumentUri(modelId));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : QueryModel
        {
            var query = _client.CreateDocumentQuery<T>(
                GetCollectionUri(),
                new FeedOptions { MaxItemCount = -1 })
                .Where(x => x.Type == typeof(T).Name)
                .AsDocumentQuery();

            var results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results.AsReadOnly();
        }

        public async Task<IEnumerable<T>> GetAll<T>(Expression<Func<T, bool>> predicate) where T : QueryModel
        {
            var query = _client.CreateDocumentQuery<T>(
                GetCollectionUri(),
                new FeedOptions { MaxItemCount = -1 })
                .Where(x => x.Type == typeof(T).Name)
                .Where(predicate)
                .AsDocumentQuery();

            var results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results.AsReadOnly();
        }

        public async Task<PagedResult<T>> GetPagedListAsync<T>(PagedQuery pagedQuery) where T : QueryModel
        {
            return await GetPagedListAsync<T>(pagedQuery, null);
        }

        public async Task<PagedResult<T>> GetPagedListAsync<T>(PagedQuery pagedQuery, Expression<Func<T, bool>> predicate) where T : QueryModel
        {
            var options = new FeedOptions
            {
                MaxItemCount = pagedQuery.MaxItemCount
            };

            if (!string.IsNullOrEmpty(pagedQuery.ContinuationToken))
            {
                options.RequestContinuation = pagedQuery.ContinuationToken;
            }

            var query = _client.CreateDocumentQuery<T>(GetCollectionUri(), options)
                .Where(x => x.Type == typeof(T).Name);
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var results = await query.AsDocumentQuery().ExecuteNextAsync<T>();

            var count = await QueryCountAsync<T>();

            return new PagedResult<T>(results.ToList().AsReadOnly(), count, results.ResponseContinuation);
        }

        public async Task<int> QueryCountAsync<T>()
        {
            var sqlQuery = string.Format("SELECT VALUE COUNT(1) FROM c WHERE c.Type = \"{0}\"", typeof(T).Name);
            var countQuery = _client.CreateDocumentQuery<int>(GetCollectionUri(), sqlQuery).AsDocumentQuery();

            var results = await countQuery.ExecuteNextAsync<int>();

            return results.FirstOrDefault();
        }

        public async Task<T> QueryItemAsync<T>(Expression<Func<T, bool>> predicate) where T : QueryModel
        {
            var query = _client.CreateDocumentQuery<T>(
                GetCollectionUri(),
                new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.Type == typeof(T).Name)
                .Where(predicate)
                .AsDocumentQuery();

            if (query.HasMoreResults)
            {
                //var dynamicResult = await (dynamic)query.ExecuteNextAsync().Result;

                //var results = (T) dynamicResults;

                var results = await query.ExecuteNextAsync<T>();

                return results.FirstOrDefault<T>();
            }

            return null;
        }

        public async Task<bool> DoesItemExist<T>(Guid id)
        {
            var modelId = typeof(T).Name + "-" + id;
            var sqlQuery = string.Format("SELECT VALUE COUNT(1) FROM c WHERE c.id = \"{0}\"", modelId);
            var countQuery = _client.CreateDocumentQuery<int>(GetCollectionUri(), sqlQuery).AsDocumentQuery();

            var results = await countQuery.ExecuteNextAsync<int>();

            return results.Count > 0 && results.FirstOrDefault() > 0;
        }

        public async Task Save(QueryModel queryModel)
        {
            await _client.UpsertDocumentAsync(GetCollectionUri(), queryModel, null, disableAutomaticIdGeneration: true);
        }

        // Utility Methods
        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    await _client.CreateDatabaseAsync(new Database { Id = _databaseId });
                else
                    throw;
            }
        }

        public async Task ClearCollectionAsync()
        {
            await _client.DeleteDocumentCollectionAsync(GetCollectionUri());

            // Refresh document client session
            _client.Dispose();

            _client = new DocumentClient(new Uri(_endpoint), _authKey,
                new ConnectionPolicy
                {
                    EnableEndpointDiscovery = false
                }
            );

            await CreateCollectionIfNotExistsAsync();
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(GetCollectionUri());
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_databaseId),
                        new DocumentCollection { Id = _collectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }

        private Uri GetDocumentUri(string id)
        {
            return UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);
        }

        private Uri GetCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        }
    }
}
