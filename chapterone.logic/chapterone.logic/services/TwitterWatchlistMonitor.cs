using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.models.twitter;
using chapterone.logic.extensions;
using chapterone.logic.interfaces;
using chapterone.services.interfaces;
using chapterone.services.scheduling;
using chapterone.shared;
using chapterone.shared.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace chapterone.logic.services
{
    /// <summary>
    /// Service for managing the watchlist using the Research Library twitter account
    /// </summary>
    public class TwitterWatchlistMonitor : IScheduledTask
    {
        private const string RESEARCH_LIBRARY_SCREENNAME = "ResearchLbry";

        private readonly IDatabaseRepository<TwitterWatchlistProfile> _twitterWatchlistRepo;
        private readonly ITimelineService _timelineService;
        private readonly ITwitterClient _twitter;
        private readonly IEventLogger _logger;


        /// <summary>
        /// Constructor
        /// </summary>
        public TwitterWatchlistMonitor(IDatabaseRepository<TwitterWatchlistProfile> twitterWatchlistRepo,
            ITimelineService timelineService,
            ITwitterClient twitterClient,
            IEventLogger logger)
        {
            _twitterWatchlistRepo = twitterWatchlistRepo;
            _timelineService = timelineService;
            _twitter = twitterClient;
            _logger = logger;
        }

        #region Implementing IScheduledTask

        /// <summary>
        /// This tasks run schedule
        /// </summary>
        public string Schedule => "0 0 * * *";


        /// <summary>
        /// Process the Research Library's friend list
        /// </summary>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // TODO: Implement the cancellation token

            try
            {
                var start = DateTime.UtcNow;

                // Pull the latest friend list...
                var friendList = await _twitter.GetFriendIdsAsync(RESEARCH_LIBRARY_SCREENNAME);

                if (friendList.FriendIds.Count() > 0)
                {
                    // ... pull the watchlist ...
                    var watchlist = await _twitterWatchlistRepo.QueryAsync(x => true);

                    // ... analyse differences between the lists ...
                    var differences = DifferenceAnalyser<long>.ProcessChanges(friendList.FriendIds, watchlist.Select(x => x.UserId));

                    if (differences.Added.Count() > 0)
                    {
                        // ... resolve the added users from twitter ...
                        var userList = await _twitter.GetUsersByIdsAsync(differences.Added);

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
                    { "monitor", nameof(TwitterWatchlistMonitor) }
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
                var friendIds = (await _twitter.GetFriendIdsAsync(friend.ScreenName)).FriendIds;

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
            await _timelineService.AddMessageAsync(new WatchlistAddedMessage()
            {
                AddedScreenNames = friends.Select(x => x.ToTwitterScreenName())
            });
        }

        #endregion
    }
}