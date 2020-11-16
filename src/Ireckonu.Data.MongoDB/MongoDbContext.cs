using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ireckonu.Data.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ireckonu.Data.Mongo
{
    public class MongoDbContext : IDbContext
    {
        private readonly MongoSettings _settings;
        private readonly IMongoClient _client;

        public MongoDbContext(MongoSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(settings.CollectionName))
            {
                throw new ArgumentException(nameof(settings), $"{nameof(settings.CollectionName)} cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ArgumentException(nameof(settings), $"{nameof(settings.ConnectionString)} cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(settings.DbName))
            {
                throw new ArgumentException(nameof(settings), $"{nameof(settings.DbName)} cannot be empty");
            }

            _client = new MongoClient(settings.ConnectionString);
        }

        private async Task<IMongoCollection<ArticleDocument>> GetCollection()
        {
            var db = _client.GetDatabase(_settings.DbName);
            var collectionSettings = new MongoCollectionSettings
            {
                WriteConcern = WriteConcern.Unacknowledged
            };

            var collection = db.GetCollection<ArticleDocument>(_settings.CollectionName, collectionSettings);

            if (collection == null)
            {
                await db.CreateCollectionAsync(_settings.CollectionName).ConfigureAwait(false);

                collection = db.GetCollection<ArticleDocument>(_settings.CollectionName, collectionSettings);
            }
            return collection;
        }

        public async Task BulkUpsert(IEnumerable<Article> articles)
        {
            var collection = await GetCollection().ConfigureAwait(false);

            var bulk = articles.Select(article =>
            {
                var document = new ArticleDocument(article);
                var key = new BsonDocument("_id", document._id);
                var upsert = new ReplaceOneModel<ArticleDocument>(key, document) 
                { 
                    IsUpsert = true 
                };
                return upsert;
            }).ToList();

            if (bulk.Count == 0)
            {
                return;
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false,
                BypassDocumentValidation = true
            };

            await collection.BulkWriteAsync(bulk, options).ConfigureAwait(false);
        }

        public void Dispose()
        {
        }
    }
}
