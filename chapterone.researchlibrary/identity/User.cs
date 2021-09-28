using chapterone.data.shared;
using Microsoft.AspNetCore.Identity;
using NodaTime;
using System;

namespace chapterone.web.identity
{
    public class User : IdentityUser, IEntity
    {
        public ZonedDateTime Created { get; set; } = LocalDateTime.FromDateTime(DateTime.UtcNow).InUtc();
        public int SchemaVersion { get; set; }
    }
}
