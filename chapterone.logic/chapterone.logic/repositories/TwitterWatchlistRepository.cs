using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.mongodb;
using chapterone.logic.interfaces;

namespace chapterone.logic.repositories
{
    public class TwitterWatchlistRepository : MongoDbRepository<TwitterWatchlistProfile>, ITwitterWatchlistRepository
    {
        public TwitterWatchlistRepository(IDatabaseSettings settings)
            : base(settings, "TwitterWatchlist")
        {
        }
    }
}
