using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.mongodb;

namespace chapterone.data.repositories
{
    public class UserRepository : MongoDbRepository<User>, IUserRepository
    {
        public UserRepository(IDatabaseSettings settings)
            : base(settings, "User")
        {
        }
    }
}
