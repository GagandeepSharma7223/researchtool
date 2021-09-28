using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using chapterone.data.interfaces;
using chapterone.data.shared;
using chapterone.services.interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace chapterone.data.mongodb
{
    public class MongoDbRepository<T> : IDatabaseRepository<T> where T : IEntity
    {
        private readonly int _schemaVersion;
        private readonly string _collectionName;

        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private IMongoCollection<T> _collection;
        private readonly IEventLogger _logger;

        private readonly bool _migrationMode;

        public MongoDbRepository(IDatabaseConfig config, int schemaVersion, IEventLogger logger, string collectionName = null, bool migrationMode = false)
        {
            _schemaVersion = schemaVersion;
            _collectionName = collectionName ?? typeof(T).Name;
            _client = new MongoClient(config.Endpoint);
            _database = _client.GetDatabase(config.Database);
            _migrationMode = migrationMode;
            _logger = logger;
        }

        public static async Task<IDatabaseRepository<T>> CreateRepository(IDatabaseConfig config, int schemaVersion, IEventLogger logger, string collectionName = null)
        {
            var repo = new MongoDbRepository<T>(config, schemaVersion, logger, collectionName);
            return await repo.InitialiseAsync();
        }

        public static async Task<IDatabaseRepository<T>> CreateRepositoryForMigration(IDatabaseConfig config, int schemaVersion, IEventLogger logger, string collectionName = null)
        {
            var repo = new MongoDbRepository<T>(config, schemaVersion, logger, collectionName, migrationMode: true);
            return await repo.InitialiseAsync();
        }

        public async Task<IDatabaseRepository<T>> InitialiseAsync()
        {
            if (!await IsExistingCollection())
                await _database.CreateCollectionAsync(_collectionName);

            _collection = _database.GetCollection<T>(_collectionName);

            // Do we have any data at the wrong schema version?
            if (_schemaVersion > -1)
            {
                var count = await _collection.CountAsync(x => x.SchemaVersion != _schemaVersion);

                if (count > 0 && !_migrationMode)
                    throw new SchemaMismatchException(_collectionName, count);
            }

            return this;
        }

        public async Task DeleteAsync(T item)
        {
            try
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item), "Given item not valid");

                await DeleteAsync(item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogEvent("MongoDbRepository Exception", new Dictionary<string, string> { { "DeleteAsync()", ex.Message } });
                throw ex;
            }
        }

        public async Task DeleteAsync(string itemId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(itemId))
                    throw new ArgumentNullException(nameof(itemId), "Given ID not valid");

                var result = await _collection.DeleteOneAsync(x => x.Id == itemId);

                if (!result.IsAcknowledged)
                    throw new MongoDbException("Delete NOT acknowledged");
            }
            catch (Exception ex)
            {
                _logger.LogEvent("MongoDbRepository Exception", new Dictionary<string, string> { { "DeleteAsync2()", ex.Message } });
                throw ex;
            }
        }

        public async Task<T> GetByIdAsync(string itemId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(itemId))
                    throw new ArgumentNullException(nameof(itemId), "Given ID not valid");

                var result = await _collection.FindAsync(x => x.Id == itemId);
                return result.First();
            }
            catch (Exception ex)
            {
                _logger.LogEvent("MongoDbRepository Exception", new Dictionary<string, string> { { "GetByIdAsync()", ex.Message } });
                throw ex;
            }
        }

        public async Task InsertAsync(T item)
        {
            try
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                item.SchemaVersion = _schemaVersion;

                await _collection.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogEvent("MongoDbRepository Exception", new Dictionary<string, string> { { "InsertAsync()", ex.Message } });
                throw ex;
            }
        }

        public async Task UpdateAsync(T item)
        {
            try
            {
                if (item == null) throw new ArgumentNullException(nameof(item));

                // We need to allow the potential for newer schema versions to be written for the purposes of migration
                if (_schemaVersion > -1 && item.SchemaVersion < _schemaVersion)
                    throw new SchemaMismatchException(_collectionName);

                var containsId = await ContainsAsync(item.Id);

                if (!containsId)
                    throw new KeyNotFoundException();

                // Stash the original concurrency stamp.  If this operation fails, we can restore it (by ref) before
                // throwing the appropriate exception
                var stamp = item.ConcurrencyStamp;
                item.ConcurrencyStamp = Guid.NewGuid().ToString();

                var result = await _collection.ReplaceOneAsync(x => x.Id == item.Id && x.ConcurrencyStamp == stamp, item);

                if (!result.IsAcknowledged)
                {
                    item.ConcurrencyStamp = stamp;
                    throw new MongoDbException("Update NOT acknowledged");
                }

                if (result.MatchedCount == 0)
                {
                    item.ConcurrencyStamp = stamp;
                    throw new LostUpdateException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogEvent("MongoDbRepository Exception", new Dictionary<string, string> { { "UpdateAsync()", ex.Message } });
                throw ex;
            }
        }

        public async Task<long> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                if (predicate == null)
                    throw new ArgumentNullException(nameof(predicate));

                return await _collection.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogEvent("MongoDbRepository Exception", new Dictionary<string, string> { { "CountAsync()", ex.Message } });
                throw ex;
            }
        }

        public async Task<bool> ContainsAsync(string itemId)
        {
            try
            {
                return await CountAsync(x => x.Id == itemId) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogEvent("MongoDbRepository Exception", new Dictionary<string, string> { { "ContainsAsync()", ex.Message } });
                throw ex;
            }
        }

        public async Task<IList<T>> QueryAsync(Expression<Func<T, bool>> predicate, int pageNumber = 0, int pageSize = 0)
        {
            try
            {
                var query = _collection.AsQueryable().Where(predicate);

                if (pageNumber > 0 && pageSize > 0)
                    query = query.Skip(pageNumber * pageSize);

                if (pageSize > 0)
                    query = query.Take(pageSize);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogEvent("MongoDbRepository Exception", new Dictionary<string, string> { { "QueryAsync()", ex.Message } });
                throw ex;
            }
        }

        public async Task<IList<T>> QueryAsync<K>(Expression<Func<T, bool>> predicate, Expression<Func<T, K>> keySelector, bool isAscending = true, int pageNumber = 0, int pageSize = 0)
        {
            try
            {
                var query = _collection.AsQueryable().Where(predicate);

                query = isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);

                if (pageNumber > 0 && pageSize > 0)
                    query = query.Skip(pageNumber * pageSize);

                if (pageSize > 0)
                    query = query.Take(pageSize);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogEvent("MongoDbRepository Exception", new Dictionary<string, string> { { "QueryAsync2()", ex.Message } });
                throw ex;
            }
        }

        public IQueryable<T> AsQueryable()
        {
            return _collection.AsQueryable();
        }

        private async Task<bool> IsExistingCollection()
        {
            var collections = await _database.ListCollectionsAsync();

            while (await collections.MoveNextAsync())
            {
                foreach (var item in collections.Current)
                {
                    var name = item.AsBsonDocument.GetElement("name").Value.ToString();
                    if (name == _collectionName)
                        return true;
                }
            }

            return false;
        }
    }
}
