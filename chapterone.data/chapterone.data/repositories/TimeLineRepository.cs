using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.mongodb;

namespace chapterone.data.repositories
{
    public class TimeLineRepository : MongoDbRepository<Message>, ITimeLineRepository
    {
        public TimeLineRepository(IDatabaseSettings settings)
            : base(settings, "Timeline")
        {
        }
    }
}
