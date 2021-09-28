using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace chapterone.services.interfaces
{
    /// <summary>
    /// Interface for an object that communicates with the Twitter API
    /// </summary>
    public interface ITwitterClient
    {
        Task<IEnumerable<ITwitterUser>> GetUsersByIdsAsync(params long[] userIds);

        Task<IEnumerable<ITwitterUser>> GetUsersByIdsAsync(IEnumerable<long> userIds);

        Task<ITwitterUser> GetUserByScreenNameAsync(string screenName);

        Task<ITwitterFriendList> GetFriendIdsAsync(string screenName);
        Task<ITwitterFriendList> GetFriendIdsAsync(long userId);
    }

    /// <summary>
    /// Interface for a twitter user
    /// </summary>
    public interface ITwitterUser
    {
        long UserId { get; set; }
        string BannerImageUri { get; set; }
        string ProfileImageUri { get; set; }
        string ScreenName { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        bool IsFollowing { get; set; }
    }

    /// <summary>
    /// Interface for a twitter friend list
    /// </summary>
    public interface ITwitterFriendList
    {
        IEnumerable<long> FriendIds { get; set; }
    }
}
