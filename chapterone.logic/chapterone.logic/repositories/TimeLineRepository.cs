using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.mongodb;
using chapterone.logic.interfaces;

namespace chapterone.logic.repositories
{
    public class TimeLineRepository : MongoDbRepository<Message>, ITimeLineRepository
    {
        public TimeLineRepository(IDatabaseSettings settings)
            : base(settings, "Timeline")
        {
        }
    }
}
