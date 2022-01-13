using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.mongodb;

namespace chapterone.data.repositories
{
    public class UserRoleRepository : MongoDbRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(IDatabaseSettings settings)
            : base(settings, "UserRole")
        {
        }
    }
}
