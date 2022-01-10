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
}
