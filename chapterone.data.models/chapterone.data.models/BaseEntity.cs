using chapterone.data.shared;
using NodaTime;
using System;

namespace chapterone.data.models
{
    public class BaseEntity : IEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ZonedDateTime Created { get; set; } = LocalDateTime.FromDateTime(DateTime.UtcNow).InUtc();
        public int SchemaVersion { get; set; } = 0;
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}
