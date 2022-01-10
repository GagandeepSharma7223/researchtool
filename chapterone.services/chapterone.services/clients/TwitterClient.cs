using chapterone.data.interfaces;
using chapterone.services.extensions;
using chapterone.services.interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace chapterone.services.clients
{
    /// <summary>
    /// Client that communicates with Twitter
    /// </summary>
    public class TwitterClient : ITwitterClient
    {
        private readonly Tweetinvi.TwitterClient _userClient;
        /// <summary>
        /// Constructor
        /// </summary>
        public TwitterClient(ITwitterSettings settings)
        {
            var credentials = new TwitterCredentials(settings.ConsumerKey, settings.ConsumerSecret, settings.AccessToken, settings.AccessTokenSecret);
            _userClient = new Tweetinvi.TwitterClient(credentials);
        }


        public async Task<ITwitterFriendList> GetFriendIdsAsync(string screenName)
        {
            await WaitIfGetFriendIdsIsAtLimit();

            // WARNING: Limited to 5000 friends ids
            var ids = await _userClient.Users.GetFriendIdsAsync(screenName);

            return new TwitterFriendList()
            {
                FriendIds = ids
            };
        }


        public async Task<ITwitterFriendList> GetFriendIdsAsync(long userId)
        {
            await WaitIfGetFriendIdsIsAtLimit();

            // WARNING: Limited to 5000 friends ids
            var ids = await _userClient.Users.GetFriendIdsAsync(userId);

            return new TwitterFriendList()
            {
                FriendIds = ids
            };
        }


        public async Task<ITwitterUser> GetUserByScreenNameAsync(string screenName)
        {
            await WaitIfGetUserFromScreenNameIsAtLimit();

            var user = await _userClient.Users.GetUserAsync(screenName);

            return user?.ToTwitterUser();
        }


        public async Task<IEnumerable<ITwitterUser>> GetUsersByIdsAsync(params long[] userIds)
        {
            return await GetUsersByIdsAsync(userIds);
        }

        public async Task<IEnumerable<ITwitterUser>> GetUsersByIdsAsync(IEnumerable<long> userIds)
        {
            await WaitIfGetUsersByIdsIsAtLimit();

            var users = await _userClient.Users.GetUsersAsync(userIds);

            return users.Select(user => user.ToTwitterUser());
        }

        #region Private methods

        /// <summary>
        /// Wait until the rate limit for the /friends/ids Twitter API
        /// </summary>
        private async Task WaitIfGetFriendIdsIsAtLimit()
        {
            var ratelimits = await GetRateLimitAsync();

            while (ratelimits.FriendsIdsLimit.Remaining <= 0)
            {
                await Task.Delay((int)ratelimits.FriendsIdsLimit.ResetDateTimeInMilliseconds);

                ratelimits = await GetRateLimitAsync();
            }
        }


        /// <summary>
        /// Wait until the rate limit for the /users/show Twitter API
        /// </summary>
        private async Task WaitIfGetUserFromScreenNameIsAtLimit()
        {
            var ratelimits = await GetRateLimitAsync();

            while (ratelimits.UsersShowIdLimit.Remaining <= 0)
            {
                await Task.Delay((int)ratelimits.UsersShowIdLimit.ResetDateTimeInMilliseconds);

                ratelimits = await GetRateLimitAsync();
            }
        }


        /// <summary>
        /// Wait until the rate limit for the /users/lookup Twitter API
        /// </summary>
        private async Task WaitIfGetUsersByIdsIsAtLimit()
        {
            var ratelimits = await GetRateLimitAsync();

            while (ratelimits.UsersLookupLimit.Remaining <= 0)
            {
                await Task.Delay((int)ratelimits.UsersLookupLimit.ResetDateTimeInMilliseconds);

                ratelimits = await GetRateLimitAsync();
            }
        }


        /// <summary>
        /// Get the API rate limit data for the current twitter credentials
        /// </summary>
        private Task<ICredentialsRateLimits> GetRateLimitAsync()
        {
            return _userClient.RateLimits.GetRateLimitsAsync();
        }

        #endregion
    }

    public class TwitterUser : ITwitterUser
    {
        public long UserId { get; set; }
        public string BannerImageUri { get; set; }
        public string ProfileImageUri { get; set; }
        public string ScreenName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsFollowing { get; set; }
    }

    public class TwitterFriendList : ITwitterFriendList
    {
        public IEnumerable<long> FriendIds { get; set; } = new long[] { };
    }
}
