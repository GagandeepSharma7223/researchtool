using chapterone.data.models;
using System.Threading.Tasks;

namespace chapterone.data.interfaces
{
    /// <summary>
    /// Interface for a service that manages the timeline
    /// </summary>
    public interface ITimelineService
    {
        /// <summary>
        /// Add the given message to the timeline
        /// </summary>
        Task AddMessageAsync(Message message);
    }
}
