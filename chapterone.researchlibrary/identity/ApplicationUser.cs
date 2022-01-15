using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using NodaTime;
using System;

namespace chapterone.web.identity
{
    [CollectionName("User")]
    public class ApplicationUser : MongoIdentityUser<string>
    {
        public ApplicationUser() : base()
        {
        }

        public ApplicationUser(string userName, string email) : base(userName, email)
        {
        }
        public string Name { get; set; }
        public ZonedDateTime Created { get; set; } = LocalDateTime.FromDateTime(DateTime.UtcNow).InUtc();
    }

    [CollectionName("Role")]
    public class ApplicationRole : MongoIdentityRole<string>
    {
        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }
    }
}
