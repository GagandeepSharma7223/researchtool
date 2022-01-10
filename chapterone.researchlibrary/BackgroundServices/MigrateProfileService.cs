using chapterone.data.interfaces;
using chapterone.services.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
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
        public MigrateProfileService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceScope = _serviceProvider.CreateScope();
            _twitterWatchlistRepo = _serviceScope.ServiceProvider.GetService<ITwitterWatchlistRepository>();
            _twitterClient = _serviceScope.ServiceProvider.GetService<ITwitterClient>();
        }

        #endregion

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await MigrateProfilesAsync();
                //Run this task once every 15 minutes
                await Task.Delay(15 * 60 * 1000, stoppingToken);
            }
        }

        private async Task MigrateProfilesAsync()
        {
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
        }

    }
}
