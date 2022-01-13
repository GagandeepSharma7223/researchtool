using chapterone.data.interfaces;
using chapterone.services.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace chapterone.web.BackgroundServices
{
    public class MigrateProfileService : BackgroundService
    {
        #region Initialize
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _serviceScope;
        private readonly ITwitterWatchlistRepository _twitterWatchlistRepo;
        private readonly ITwitterClient _twitterClient;
        private readonly IEventLogger _logger;
        public MigrateProfileService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceScope = _serviceProvider.CreateScope();
            _twitterWatchlistRepo = _serviceScope.ServiceProvider.GetService<ITwitterWatchlistRepository>();
            _twitterClient = _serviceScope.ServiceProvider.GetService<ITwitterClient>();
            _logger = _serviceScope.ServiceProvider.GetService<IEventLogger>();
        }

        #endregion

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int hours = 6, mins = 15;
                TimeSpan serviceToRunAt = new TimeSpan(hours, mins, 0); //6:15AM
                TimeSpan timeNow = DateTime.Now.TimeOfDay;
                if (timeNow > serviceToRunAt)
                {
                    await RunProcessAsync();
                    //Run this task once every day
                    var now = DateTime.Now;
                    var tomorrow = DateTime.Today.AddDays(1).AddHours(hours).AddMinutes(mins);
                    var timeSpan = tomorrow - now;
                    await Task.Delay(timeSpan, stoppingToken);
                }
                else
                {
                    await Task.Delay(serviceToRunAt - DateTime.Now.TimeOfDay);
                }
            }
        }

        private async Task RunProcessAsync()
        {
            try
            {
                var start = DateTime.UtcNow;
                const int WATCHLIST_SCHEMAVERSION = 6;

                var profiles = await _twitterWatchlistRepo.QueryAsync(x => x.SchemaVersion != WATCHLIST_SCHEMAVERSION);

                foreach (var profile in profiles)
                {
                    var user = await _twitterClient.GetUserByScreenNameAsync(profile.ScreenName);

                    profile.BannerImageUri = user.BannerImageUri;
                    profile.ProfileImageUri = user.ProfileImageUri;
                    profile.Name = user.Name;
                    profile.Biography = user.Description;
                    profile.SchemaVersion = WATCHLIST_SCHEMAVERSION;
                    await _twitterWatchlistRepo.UpdateAsync(profile);
                }

                var end = DateTime.UtcNow;
                var duration = end.Subtract(start);
                _logger.LogEvent(nameof(MigrateProfileService), new Dictionary<string, string>()
                {
                    { "start", start.ToString() },
                    { "end", end.ToString() },
                    { "duration", duration.ToString() }
                });
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, new Dictionary<string, string>()
                {
                    { "monitor", nameof(MigrateProfileService) }
                });
            }
        }
    }
}
