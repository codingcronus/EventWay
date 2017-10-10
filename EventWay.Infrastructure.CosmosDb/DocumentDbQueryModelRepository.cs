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
        private const string PartitionKeyPath = "/partitionKey";

        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly int _offerThroughput;
        private readonly int _noOfPartitions;
        private readonly string _endpoint;
        private readonly string _authKey;
        private DocumentClient _client;

        public DocumentDbQueryModelRepository(string database, string collection, int offerThroughput, int noOfPartitions, string endpoint, string authKey)
        {
            _databaseId = database;
            _collectionId = collection;
            _offerThroughput = offerThroughput;
            _noOfPartitions = noOfPartitions;
            _endpoint = endpoint;
            _authKey = authKey;

            CreateClient();
        }

        private void CreateClient()
        {
            var policy = new ConnectionPolicy { EnableEndpointDiscovery = false };
            _client = new DocumentClient(new Uri(_endpoint), _authKey, policy);
        }

        public void Initialize()
        {
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        public async Task Save(QueryModel queryModel)
        {
            queryModel.partitionKey = PartitionKeyGenerator.Generate(queryModel.id, _noOfPartitions);
            await DocumentDbRetryPolicy.ExecuteWithRetries(
                () => _client.UpsertDocumentAsync(GetCollectionUri(), queryModel, null, disableAutomaticIdGeneration: true)
                );
        }

        public async Task DeleteById<T>(Guid id) where T : QueryModel
        {
            var options = CreateRequestOptions(id);

            await DocumentDbRetryPolicy.ExecuteWithRetries(
                () => _client.DeleteDocumentAsync(GetDocumentUri(id), options)
                );
        }

        public async Task<T> GetById<T>(Guid id) where T : QueryModel
        {
            return await DocumentDbRetryPolicy.ExecuteWithRetries(
                () => GetByIdInternal<T>(id)
                );
        }

        public async Task<T> GetByIdInternal<T>(Guid id) where T : QueryModel
        {
            try
            {
                var options = CreateRequestOptions(id);

                var document = await _client.ReadDocumentAsync(GetDocumentUri(id), options);
                return (T)(dynamic)document.Resource;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        private RequestOptions CreateRequestOptions(Guid id)
        {
            var key = PartitionKeyGenerator.Generate(id, _noOfPartitions);

            var options = new RequestOptions()
            {
                PartitionKey = new PartitionKey(key)
            };

            return options;
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : QueryModel
        {
            var options = CreateFeedOptions(-1);
            var query = _client.CreateDocumentQuery<T>(GetCollectionUri(), options)
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
            var options = CreateFeedOptions(-1);
            var query = _client.CreateDocumentQuery<T>(GetCollectionUri(), options)
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
            return await DocumentDbRetryPolicy.ExecuteWithRetries(
              () => GetPagedListAsyncInternal<T>(pagedQuery, predicate)
              );
        }

        private async Task<PagedResult<T>> GetPagedListAsyncInternal<T>(PagedQuery pagedQuery, Expression<Func<T, bool>> predicate) where T : QueryModel
        {
            var options = CreateFeedOptions(pagedQuery.MaxItemCount);
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

            var count = await QueryCountAsyncInternal<T>();

            return new PagedResult<T>(results.ToList().AsReadOnly(), count, results.ResponseContinuation);
        }

        public async Task<int> QueryCountAsync<T>() where T : QueryModel
        {
            return await DocumentDbRetryPolicy.ExecuteWithRetries(
              () => QueryCountAsyncInternal<T>()
              );
        }

        private async Task<int> QueryCountAsyncInternal<T>() where T : QueryModel
        {
            var options = CreateFeedOptions(1);
            var countQuery = _client.CreateDocumentQuery<T>(GetCollectionUri(), options);
            return await countQuery.CountAsync();
        }

        public async Task<T> QueryItemAsync<T>(Expression<Func<T, bool>> predicate) where T : QueryModel
        {
            return await DocumentDbRetryPolicy.ExecuteWithRetries(
               () => QueryItemAsyncInternal<T>(predicate)
               );
        }

        private async Task<T> QueryItemAsyncInternal<T>(Expression<Func<T, bool>> predicate) where T : QueryModel
        {
            var options = CreateFeedOptions(1);
            var query = _client.CreateDocumentQuery<T>(GetCollectionUri(), options)
                .Where(x => x.Type == typeof(T).Name)
                .Where(predicate)
                .AsDocumentQuery();

            if (query.HasMoreResults)
            {
                var results = await query.ExecuteNextAsync<T>();
                return results.FirstOrDefault();
            }

            return null;
        }

        private FeedOptions CreateFeedOptions(int maxItemCount)
        {
            return new FeedOptions
            {
                MaxItemCount = maxItemCount,
                EnableCrossPartitionQuery = true,
                MaxDegreeOfParallelism = -1,
                MaxBufferedItemCount = -1
            };
        }

        public async Task<bool> DoesItemExist<T>(Guid id) where T : QueryModel
        {
            return await DocumentDbRetryPolicy.ExecuteWithRetries(
                () => DoesItemExistInternal<T>(id)
                );
        }

        private async Task<bool> DoesItemExistInternal<T>(Guid id) where T : QueryModel
        {
            var options = CreateFeedOptions(1);
            var countQuery = _client.CreateDocumentQuery<T>(GetCollectionUri(), options).Where(p => p.id == id);
            var count = await countQuery.CountAsync();

            return count > 0;
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

            CreateClient();

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
                    var database = UriFactory.CreateDatabaseUri(_databaseId);
                    var collection = new DocumentCollection { Id = _collectionId };
                    collection.PartitionKey.Paths.Add(PartitionKeyPath);

                    var options = new RequestOptions { OfferThroughput = _offerThroughput };
                    await _client.CreateDocumentCollectionAsync(database, collection, options);
                }
                else
                {
                    throw;
                }
            }
        }

        private Uri GetDocumentUri(Guid id)
        {
            return UriFactory.CreateDocumentUri(_databaseId, _collectionId, id.ToString());
        }

        private Uri GetCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        }
    }
}
