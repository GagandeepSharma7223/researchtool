using chapterone.data.repositories;

namespace chapterone.data.models
{
    /// <summary>
    /// Model for a twitter account on the watchlist
    /// </summary>
    public class TwitterWatchlistProfile : BaseEntity
    {
        /// <summary>
        /// The ID for a twitter account
        /// </summary>
        public long UserId { get; set; }


        /// <summary>
        /// The URI to the banner image
        /// </summary>
        public string BannerImageUri { get; set; }


        /// <summary>
        /// The URI to the profile image
        /// </summary>
        public string ProfileImageUri { get; set; }


        /// <summary>
        /// The screen name for the twitter account
        /// </summary>
        public string ScreenName => Id;


        /// <summary>
        /// The twitter user's name
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// The user's biography
        /// </summary>
        public string Biography { get; set; }


        /// <summary>
        /// Last-known list of friend IDs for the twitter account
        /// </summary>
        public long[] FriendIds { get; set; }
    }
}
