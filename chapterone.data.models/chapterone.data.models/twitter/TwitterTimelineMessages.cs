using chapterone.data.models.enums;
using System.Collections.Generic;

namespace chapterone.data.models.twitter
{
    /// <summary>
    /// POCO for Timeline messages that contain twitter screen name information
    /// </summary>
    public class TwitterScreenName
    {
        /// <summary>
        /// URI to the user's background image
        /// </summary>
        public string BannerImageUri { get; set; }


        /// <summary>
        /// URI to the user's avatar
        /// </summary>
        public string AvatarUri { get; set; }


        /// <summary>
        /// The @handle
        /// </summary>
        public string ScreenName { get; set; }


        /// <summary>
        /// The twitter user's name
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// The twitter user's biography
        /// </summary>
        public string Biography { get; set; }


        /// <summary>
        /// Are they already friends of Research Library?
        /// </summary>
        public bool IsFriend { get; set; }
    }


    /// <summary>
    /// Timeline message for when a watchlist profile follows people
    /// </summary>
    public class WatchlistFriendsFollowingMessage : Message
    {
        /// <summary>
        /// URI to the twitter user's avatar
        /// </summary>
        public string ProfileAvatarUri { get; set; }


        /// <summary>
        /// Screen name of the user this message is for
        /// </summary>
        public string ProfileScreenName { get; set; }


        /// <summary>
        /// Newly followed friends
        /// </summary>
        public IEnumerable<TwitterScreenName> FollowedScreenNames { get; set; } = new TwitterScreenName[] { };


        /// <summary>
        /// Constructor
        /// </summary>
        public WatchlistFriendsFollowingMessage()
            : base(MessageType.TwitterWatchlistFriendsFollowed)
        {
        }
    }


    /// <summary>
    /// Timeline message for when a watchlist profile unfollows people
    /// </summary>
    public class WatchlistFriendsUnfollowingMessage : Message
    {
        /// <summary>
        /// URI to the twitter user's avatar
        /// </summary>
        public string ProfileAvatarUri { get; set; }


        /// <summary>
        /// Screen name of the user this message is for
        /// </summary>
        public string ProfileScreenName { get; set; }


        /// <summary>
        /// Newly unfollowed friends
        /// </summary>
        public IEnumerable<TwitterScreenName> UnfollowedScreenNames { get; set; } = new TwitterScreenName[] { };


        /// <summary>
        /// Constructor
        /// </summary>
        public WatchlistFriendsUnfollowingMessage()
            : base(MessageType.TwitterWatchlistFriendsUnfollowed)
        {
        }
    }


    /// <summary>
    /// Timeline message for when a profile is added to the watchlist
    /// </summary>
    public class WatchlistAddedMessage : Message
    {
        /// <summary>
        /// Screen name of the profile added to the watchlist
        /// </summary>
        public IEnumerable<TwitterScreenName> AddedScreenNames { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public WatchlistAddedMessage()
            : base(MessageType.TwitterWatchlistProfileAdded)
        {
        }
    }


    /// <summary>
    /// Timeline message for when a profile is removed from the watchlist
    /// </summary>
    public class WatchlistRemovedMessage : Message
    {
        /// <summary>
        /// Screen name of the profile removed from the watchlist
        /// </summary>
        public IEnumerable<TwitterScreenName> RemovedScreenNames { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public WatchlistRemovedMessage()
            : base(MessageType.TwitterWatchlistProfileRemoved)
        {
        }
    }
}
