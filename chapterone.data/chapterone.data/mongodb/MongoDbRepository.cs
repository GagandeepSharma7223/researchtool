using chapterone.data.interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace chapterone.data.mongodb
{
    public class MongoDbRepository<T> : IDatabaseRepository<T> where T : IEntity
    {
        const int _schemaVersion = 6;
        private readonly string _collectionName;
        private readonly IMongoDatabase _database;
        private IMongoCollection<T> _collection;

        public MongoDbRepository(IDatabaseSettings settings, string collectionName = null)
        {
            _database = new MongoClient(settings.ConnectionString).GetDatabase(settings.Name);
            _collection = _database.GetCollection<T>(collectionName);
            _collectionName = collectionName;
        }

        public async Task DeleteAsync(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Given item not valid");

            await DeleteAsync(item.Id);
        }

        public async Task DeleteAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                throw new ArgumentNullException(nameof(itemId), "Given ID not valid");

            var result = await _collection.DeleteOneAsync(x => x.Id == itemId);

            if (!result.IsAcknowledged)
                throw new MongoDbException("Delete NOT acknowledged");
        }

        public async Task<T> GetByIdAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                throw new ArgumentNullException(nameof(itemId), "Given ID not valid");

            var result = await _collection.FindAsync(x => x.Id == itemId);
            return result.First();
        }

        public async Task InsertAsync(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            item.SchemaVersion = _schemaVersion;
            await _collection.InsertOneAsync(item);
        }

        public async Task UpdateAsync(T item)
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

        public async Task<long> CountAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            return await _collection.CountAsync(predicate);
        }

        public async Task<bool> ContainsAsync(string itemId)
        {
            return await CountAsync(x => x.Id == itemId) > 0;
        }

        public async Task<IList<T>> QueryAsync(Expression<Func<T, bool>> predicate, int pageNumber = 0, int pageSize = 0)
        {
            var query = _collection.AsQueryable().Where(predicate);

            if (pageNumber > 0 && pageSize > 0)
                query = query.Skip(pageNumber * pageSize);

            if (pageSize > 0)
                query = query.Take(pageSize);

            return await query.ToListAsync();
        }

        public async Task<IList<T>> QueryAsync<K>(Expression<Func<T, bool>> predicate, Expression<Func<T, K>> keySelector, bool isAscending = true, int pageNumber = 0, int pageSize = 0)
        {
            var query = _collection.AsQueryable().Where(predicate);

            query = isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);

            if (pageNumber > 0 && pageSize > 0)
                query = query.Skip(pageNumber * pageSize);

            if (pageSize > 0)
                query = query.Take(pageSize);

            return await query.ToListAsync();
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
