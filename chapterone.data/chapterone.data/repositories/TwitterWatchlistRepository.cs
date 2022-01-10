using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.mongodb;

namespace chapterone.data.repositories
{
    public class TwitterWatchlistRepository : MongoDbRepository<TwitterWatchlistProfile>, ITwitterWatchlistRepository
    {
        public TwitterWatchlistRepository(IDatabaseSettings settings)
            : base(settings, "TwitterWatchlist")
        {
        }
    }
}
