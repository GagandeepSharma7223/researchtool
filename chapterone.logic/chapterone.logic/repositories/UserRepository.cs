using chapterone.data.interfaces;
using chapterone.data.mongodb;
using chapterone.logic.interfaces;

namespace chapterone.logic.repositories
{
    public class UserRepository : MongoDbRepository<User>, IUserRepository
    {
        public UserRepository(IDatabaseSettings settings)
            : base(settings, "User")
        {
        }
    }
}
