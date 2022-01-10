namespace chapterone.data.enums
{
    /// <summary>
    /// Types of messages on the timeline
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Default
        /// </summary>
        None = 0,


        /// <summary>
        /// A new profile is added to the watchlist
        /// </summary>
        TwitterWatchlistProfileAdded = 1,


        /// <summary>
        /// A profile is removed from the watchlist
        /// </summary>
        TwitterWatchlistProfileRemoved = 2,


        /// <summary>
        /// A profile on the watchlist is following new people
        /// </summary>
        TwitterWatchlistFriendsFollowed = 3,


        /// <summary>
        /// A profile on the watchlist has unfollowed people
        /// </summary>
        TwitterWatchlistFriendsUnfollowed = 4
    }
}
