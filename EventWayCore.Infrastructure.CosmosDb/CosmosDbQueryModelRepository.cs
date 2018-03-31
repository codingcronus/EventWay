using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EventWayCore.Query;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace EventWayCore.Infrastructure.CosmosDb
{
    public class CosmosDbQueryModelRepository : IQueryModelRepository
    {
        private const string PartitionKeyPath = "/AggregateId";
        private static Uri _collectionUri;
        private readonly object _lockUri = new object();
        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly int _offerThroughput;
        private readonly int _noOfParallelTasks;

        private readonly string _endpoint;
        private readonly string _authKey;
        private DocumentClient _client;

        public CosmosDbQueryModelRepository(string database, string collection, int offerThroughput, int noOfParallelTasks, string endpoint, string authKey)
        {
            _databaseId = database;
            _collectionId = collection;
            _offerThroughput = offerThroughput;
            _noOfParallelTasks = noOfParallelTasks;

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
            await DocumentDbRetryPolicy.ExecuteWithRetries(
                () => _client.UpsertDocumentAsync(GetCollectionUri(), queryModel, null, disableAutomaticIdGeneration: true)
            );
        }

        public async Task<T> GetById<T>(Guid aggregateId) where T : QueryModel
        {
            return await QueryItem<T>(x => x.AggregateId == aggregateId);
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

        public async Task<T> QueryItem<T>(Expression<Func<T, bool>> predicate) where T : QueryModel
        {
            return await DocumentDbRetryPolicy.ExecuteWithRetries(
                () => QueryItemInternal(predicate)
            );
        }

        private async Task<T> QueryItemInternal<T>(Expression<Func<T, bool>> predicate) where T : QueryModel
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

        public async Task<IEnumerable<T>> QueryAll<T>(Expression<Func<T, bool>> predicate) where T : QueryModel
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

        public async Task DeleteById<T>(Guid aggregateId) where T : QueryModel
        {
            var options = CreateRequestOptions(aggregateId);

            await DocumentDbRetryPolicy.ExecuteWithRetries(
                async () => _client.DeleteDocumentAsync(await GetDocumentUri<T>(aggregateId), options)
            );
        }


        //TODO: Should this method be deleted?
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

        /////////////////////////////////////////
        // Utility Methods
        /////////////////////////////////////////

        private RequestOptions CreateRequestOptions(Guid aggregateId)
        {
            var options = new RequestOptions()
            {
                PartitionKey = new PartitionKey(aggregateId.ToString())
            };

            return options;
        }

        private FeedOptions CreateFeedOptions(int maxItemCount, bool enableCrossPartitionQuery = true)
        {
            return new FeedOptions
            {
                MaxItemCount = maxItemCount,
                EnableCrossPartitionQuery = enableCrossPartitionQuery,
                MaxDegreeOfParallelism = -1,
                MaxBufferedItemCount = -1
            };
        }

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

        private async Task<Uri> GetDocumentUri<T>(Guid aggregateId)
        {
            var sql = "SELECT TOP 1 c.id FROM c WHERE c.AggregateId = \"" + aggregateId + "\" AND c.Type = \"" + typeof(T).Name + "\"";

            var options = CreateFeedOptions(-1);
            var query = _client.CreateDocumentQuery<dynamic>(GetCollectionUri(), sql, options).AsDocumentQuery();
            var result = await query.ExecuteNextAsync<dynamic>();

            if (result == null)
                throw new Exception("Could not find query model with type: " + typeof(T).Name + " and AggregateId: " + aggregateId);
            if (result.Count == 0)
                throw new Exception("Could not find query model with type: " + typeof(T).Name + " and AggregateId: " + aggregateId);
            if (result.Count > 1)
                throw new Exception("Found more than one query model with type: " + typeof(T).Name + " and AggregateId: " + aggregateId);

            var documentId = GetPropValue(result.First(), "id");

            UriFactory.CreateDocumentUri(_databaseId, _collectionId, documentId);

            return UriFactory.CreateDocumentUri(_databaseId, _collectionId, documentId);
        }

        public static string GetPropValue(object target, string propName)
        {
            return target.GetType().GetProperty(propName).GetValue(target, null) as string;
        }

        private Uri GetCollectionUri()
        {
            if (_collectionUri != null)
            {
                return _collectionUri;
            }
            else
            {
                lock (_lockUri)
                {
                    if (_collectionUri != null)
                    {
                        return _collectionUri;
                    }
                    else
                    {
                        _collectionUri = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                        return _collectionUri;
                    }
                }
            }
        }
    }
}