using NodaTime;

namespace chapterone.data.interfaces
{
    public interface IEntity
    {
        string Id { get; set; }

        ZonedDateTime Created { get; set; }

        int SchemaVersion { get; set; }

        string ConcurrencyStamp { get; set; }
    }
}
