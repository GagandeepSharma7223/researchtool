using chapterone.data.models.enums;
using MongoDB.Bson.Serialization.Attributes;

namespace chapterone.data.models
{
    /// <summary>
    /// Model for messages on the timeline
    /// </summary>
    [BsonKnownTypes(typeof(twitter.WatchlistFriendsFollowingMessage), 
        typeof(twitter.WatchlistFriendsUnfollowingMessage), 
        typeof(twitter.WatchlistAddedMessage),
        typeof(twitter.WatchlistRemovedMessage))]
    public class Message : BaseEntity
    {
        /// <summary>
        /// The type of message
        /// </summary>
        public MessageType Type { get; private set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public Message(MessageType type)
        {
            Type = type;
        }
    }
}
