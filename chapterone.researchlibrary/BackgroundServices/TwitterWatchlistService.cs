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
    public class TwitterWatchlistService : BackgroundService
    {
        #region Initialize
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _serviceScope;
        private readonly ITwitterWatchlistRepository _twitterWatchlistRepo;
        private readonly ITwitterClient _twitterClient;
        private readonly ITimeLineRepository _timeLineRepository;
        private readonly IEventLogger _logger;
        private const string RESEARCH_LIBRARY_SCREENNAME = "ResearchLbry";
        public TwitterWatchlistService(IServiceProvider serviceProvider)
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
                int hours = 23, mins = 55;
                TimeSpan serviceToRunAt = new TimeSpan(hours, mins, 0); //11:55PM
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

                // Pull the latest friend list...
                var friendList = await _twitterClient.GetFriendIdsAsync(RESEARCH_LIBRARY_SCREENNAME);

                if (friendList.FriendIds.Count() > 0)
                {
                    // ... pull the watchlist ...
                        var watchlist = await _twitterWatchlistRepo.QueryAsync(x => true);

                    // ... analyse differences between the lists ...
                    var differences = DifferenceAnalyser<long>.ProcessChanges(friendList.FriendIds, watchlist.Select(x => x.UserId));

                    if (differences.Added.Count() > 0)
                    {
                        // ... resolve the added users from twitter ...
                        var userList = await _twitterClient.GetUsersByIdsAsync(differences.Added);

                        // ... process them
                        await ProcessAddedFriends(userList);
                    }
                }

                var end = DateTime.UtcNow;
                var duration = end.Subtract(start);

                _logger.LogEvent(EventConstants.EVENT_WATCHLIST_MONITOR, new Dictionary<string, string>()
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
                    { "monitor", nameof(TwitterWatchlistService) }
                });
            }
        }


        /// <summary>
        /// Add the given twitter user list to the watchlist and report it to the timeline
        /// </summary>
        private async Task ProcessAddedFriends(IEnumerable<ITwitterUser> friends)
        {
            // Add new friends to the watchlist
            foreach (var friend in friends)
            {
                var friendIds = (await _twitterClient.GetFriendIdsAsync(friend.ScreenName)).FriendIds;

                // TODO: Implement InsertManyAsync...
                await _twitterWatchlistRepo.InsertAsync(new TwitterWatchlistProfile()
                {
                    Id = friend.ScreenName,
                    ProfileImageUri = friend.ProfileImageUri,
                    BannerImageUri = friend.BannerImageUri,
                    UserId = friend.UserId,
                    Biography = friend.Description,
                    Name = friend.Name,
                    FriendIds = friendIds.ToArray()
                });
            }

            // Report to the timeline...
            await _timeLineRepository.InsertAsync(new WatchlistAddedMessage()
            {
                AddedScreenNames = friends.Select(x => x.ToTwitterScreenName())
            });
        }

    }
}
