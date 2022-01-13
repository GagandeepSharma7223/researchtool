using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.mongodb;

namespace chapterone.data.repositories
{
    public class RoleRepository : MongoDbRepository<Role>, IRoleRepository
    {
        public RoleRepository(IDatabaseSettings settings)
            : base(settings, "Role")
        {
        }
    }
}
