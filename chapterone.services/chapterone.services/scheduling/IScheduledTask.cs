using System.Threading;
using System.Threading.Tasks;

namespace chapterone.services.scheduling
{
    /// <summary>
    /// Interface for a scheduled task
    /// </summary>
    public interface IScheduledTask
    {
        /// <summary>
        /// The CRON schedule string
        /// </summary>
        string Schedule { get; }


        /// <summary>
        /// Execute this task asynchronously
        /// </summary>
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
