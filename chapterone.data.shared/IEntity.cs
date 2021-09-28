using NodaTime;
using System;

namespace chapterone.data.shared
{
    public interface IEntity
    {
        string Id { get; set; }

        ZonedDateTime Created { get; set; }

        int SchemaVersion { get; set; }

        string ConcurrencyStamp { get; set; }
    }
}
