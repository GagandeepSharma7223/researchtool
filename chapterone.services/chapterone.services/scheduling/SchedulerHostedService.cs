using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chapterone.services.cron;
using chapterone.services.interfaces;

namespace chapterone.services.scheduling
{
    /// <summary>
    /// A task scheduling service
    /// </summary>
    public class SchedulerHostedService : HostedService
    {
        /// <summary>
        /// Event for when a task throws an exception
        /// </summary>
        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        private readonly List<SchedulerTaskWrapper> _scheduledTasks = new List<SchedulerTaskWrapper>();
        private readonly IEventLogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public SchedulerHostedService(IEnumerable<IScheduledTask> scheduledTasks, IEventLogger logger)
        {
            var referenceTime = DateTime.UtcNow;

            foreach (var scheduledTask in scheduledTasks)
            {
                _scheduledTasks.Add(new SchedulerTaskWrapper
                {
                    Schedule = CrontabSchedule.Parse(scheduledTask.Schedule),
                    Task = scheduledTask,
                    NextRunTime = referenceTime
                });
            }

            _logger = logger;
        }


        /// <summary>
        /// Start executing scheduled tasks
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogEvent($"{nameof(SchedulerHostedService)}:ExecuteAsync_Start");

            while (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(cancellationToken);

                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }

            _logger.LogEvent($"{nameof(SchedulerHostedService)}:ExecuteAsync_End");
        }



        /// <summary>
        /// Execute the scheduled tasks
        /// </summary>
        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var referenceTime = DateTime.UtcNow;

            var tasksThatShouldRun = _scheduledTasks.Where(t => t.ShouldRun(referenceTime)).ToList();

            _logger.LogEvent($"{nameof(SchedulerHostedService)}:ExecuteOnceAsync", new Dictionary<string, string>()
            {
                { "tasksThatShouldRun", tasksThatShouldRun.Count.ToString() },
            });

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();

                await taskFactory.StartNew(
                    async () =>
                    {
                        try
                        {
                            await taskThatShouldRun.Task.ExecuteAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            var args = new UnobservedTaskExceptionEventArgs(
                                ex as AggregateException ?? new AggregateException(ex));

                            UnobservedTaskException?.Invoke(this, args);

                            if (!args.Observed)
                            {
                                throw;
                            }
                        }
                    },
                    cancellationToken);
            }
        }

        private class SchedulerTaskWrapper
        {
            public CrontabSchedule Schedule { get; set; }
            public IScheduledTask Task { get; set; }

            public DateTime LastRunTime { get; set; }
            public DateTime NextRunTime { get; set; }

            public void Increment()
            {
                LastRunTime = NextRunTime;
                NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
            }

            public bool ShouldRun(DateTime currentTime)
            {
                return NextRunTime < currentTime && LastRunTime != NextRunTime;
            }
        }
    }
}
