using chapterone.data.interfaces;
using Microsoft.AspNetCore.Identity;
using NodaTime;
using System;

namespace chapterone.data.models
{
    public class User : IdentityUser, IEntity
    {
        public ZonedDateTime Created { get; set; } = LocalDateTime.FromDateTime(DateTime.UtcNow).InUtc();
        public int SchemaVersion { get; set; }
    }
    public class Role : IdentityRole, IEntity
    {
        public ZonedDateTime Created { get; set; } = LocalDateTime.FromDateTime(DateTime.UtcNow).InUtc();
        public int SchemaVersion { get; set; }
    }
    public class UserRole : IdentityUserRole<string>, IEntity
    {
        public ZonedDateTime Created { get; set; } = LocalDateTime.FromDateTime(DateTime.UtcNow).InUtc();
        public int SchemaVersion { get; set; }
        public virtual string Id { get; set; }
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}
