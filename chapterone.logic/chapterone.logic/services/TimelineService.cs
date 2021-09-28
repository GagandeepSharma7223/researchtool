using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.logic.interfaces;

using System;
using System.Threading.Tasks;

namespace chapterone.logic.services
{
    /// <summary>
    /// Service for managing the user's timeline
    /// </summary>
    public class TimelineService : ITimelineService
    {
        private readonly IDatabaseRepository<Message> _timelineRepo;


        /// <summary>
        /// Constructor
        /// </summary>
        public TimelineService(IDatabaseRepository<Message> timelineRepo)
        {
            _timelineRepo = timelineRepo;
        }


        /// <inheritdoc />
        public async Task AddMessageAsync(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            await _timelineRepo.InsertAsync(message);
        }
    }
}
