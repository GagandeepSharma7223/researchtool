using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.services.extensions;
using chapterone.services.interfaces;
using chapterone.services.scheduling;
using chapterone.shared;
using chapterone.shared.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace chapterone.services
{
    /// <summary>
    /// Service for processing the friend lists of everyone in the watchlist
    /// </summary>
    public class TwitterWatchlistFriendsMonitor : IScheduledTask
    {
        private readonly IDatabaseRepository<TwitterWatchlistProfile> _twitterWatchlistRepo;
        private readonly ITimelineService _timelineService;
        private readonly ITwitterClient _twitter;
        private readonly IEventLogger _logger;


        /// <summary>
        /// Constructor
        /// </summary>
        public TwitterWatchlistFriendsMonitor(IDatabaseRepository<TwitterWatchlistProfile> twitterWatchlistRepo,
            ITimelineService timelineService,
            ITwitterClient twitter,
            IEventLogger logger)
        {
            _twitterWatchlistRepo = twitterWatchlistRepo;
            _timelineService = timelineService;
            _twitter = twitter;
            _logger = logger;
        }
        
        
        #region Implementing IScheduledTask

        /// <summary>
        /// This tasks run schedule
        /// </summary>
        public string Schedule => "0 3,9,15,21 * * *";


        /// <summary>
        /// Process the friend list for all profiles in the watchlist
        /// </summary>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // TODO: Implement the cancellation token

            try
            {
                var start = DateTime.UtcNow;

                // Pull the watchlist...
                var watchlist = await _twitterWatchlistRepo.QueryAsync(x => true);

                // TODO: PAGE THIS!
                foreach (var profile in watchlist)
                {
                    // ... pull the latest friend list for the profile on the watchlist...
                    var friendList = await _twitter.GetFriendIdsAsync(profile.UserId);

                    // Twitter bug: sometimes the API returns no friends
                    if (friendList.FriendIds.Count() > 0)
                    {
                        // ... analyse the differences between old and new...
                        var differences = DifferenceAnalyser<long>.ProcessChanges(friendList.FriendIds, profile.FriendIds);

                        if (differences.Added.Count() > 0)
                        {
                            // ... resolve the new friends ...
                            var addedFriends = await _twitter.GetUsersByIdsAsync(differences.Added);

                            var message = new WatchlistFriendsFollowingMessage()
                            {
                                ProfileAvatarUri = profile.ProfileImageUri,
                                ProfileScreenName = profile.ScreenName,
                                FollowedScreenNames = addedFriends.Select(x => x.ToTwitterScreenName()),
                            };

                            await _timelineService.AddMessageAsync(message);
                        }

                        // Update the friends list on the profile
                        profile.FriendIds = friendList.FriendIds.ToArray();

                        await _twitterWatchlistRepo.UpdateAsync(profile);
                    }

                    _logger.LogEvent("TwitterFriendlistMonitor:FriendProcessed");
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
                    { "monitor", nameof(TwitterWatchlistFriendsMonitor) }
                });
            }
        }

        #endregion
    }
}
