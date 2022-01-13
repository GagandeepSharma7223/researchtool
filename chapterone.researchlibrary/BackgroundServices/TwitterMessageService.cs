using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.services.extensions;
using chapterone.services.interfaces;
using chapterone.shared;
using chapterone.shared.utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace chapterone.web.BackgroundServices
{
    public class TwitterMessageService : BackgroundService
    {
        #region Initialize
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _serviceScope;
        private readonly ITwitterWatchlistRepository _twitterWatchlistRepo;
        private readonly ITwitterClient _twitterClient;
        private readonly ITimeLineRepository _timeLineRepository;
        private readonly IEventLogger _logger;
        public TwitterMessageService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceScope = _serviceProvider.CreateScope();
            _twitterWatchlistRepo = _serviceScope.ServiceProvider.GetService<ITwitterWatchlistRepository>();
            _twitterClient = _serviceScope.ServiceProvider.GetService<ITwitterClient>();
            _timeLineRepository = _serviceScope.ServiceProvider.GetService<ITimeLineRepository>();
            _logger = _serviceScope.ServiceProvider.GetService<IEventLogger>();
        }
        #endregion
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SaveMessagesAsync();
                //Run this task once every 6 hours
                await Task.Delay(6 * 60 * 60 * 1000, stoppingToken);
            }
        }

        private async Task SaveMessagesAsync()
        {
            try
            {
                var start = DateTime.UtcNow;

                // Pull the watchlist...
                var watchlist = await _twitterWatchlistRepo.QueryAsync(x => true);

                // TODO: PAGE THIS!
                foreach (var profile in watchlist)
                {
                    // ... pull the latest friend list for the profile on the watchlist...
                    var friendList = await _twitterClient.GetFriendIdsAsync(profile.UserId);

                    // Twitter bug: sometimes the API returns no friends
                    if (friendList.FriendIds.Count() > 0)
                    {
                        // ... analyse the differences between old and new...
                        var differences = DifferenceAnalyser<long>.ProcessChanges(friendList.FriendIds, profile.FriendIds);

                        if (differences.Added.Count() > 0)
                        {
                            // ... resolve the new friends ...
                            var addedFriends = await _twitterClient.GetUsersByIdsAsync(differences.Added);

                            var message = new WatchlistFriendsFollowingMessage()
                            {
                                ProfileAvatarUri = profile.ProfileImageUri,
                                ProfileScreenName = profile.ScreenName,
                                FollowedScreenNames = addedFriends.Select(x => x.ToTwitterScreenName()),
                            };

                            await _timeLineRepository.InsertAsync(message);
                        }

                        // Update the friends list on the profile
                        profile.FriendIds = friendList.FriendIds.ToArray();

                        await _twitterWatchlistRepo.UpdateAsync(profile);
                    }

                    _logger.LogEvent("TwitterMessageService:FriendProcessed");
                }

                var end = DateTime.UtcNow;
                var duration = end.Subtract(start);

                _logger.LogEvent(EventConstants.EVENT_FRIENDLIST_MONITOR, new Dictionary<string, string>()
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
                    { "monitor", nameof(TwitterMessageService) }
                });
            }
        }
    }
}
