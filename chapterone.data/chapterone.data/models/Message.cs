using chapterone.data.enums;
using chapterone.data.repositories;
using MongoDB.Bson.Serialization.Attributes;

namespace chapterone.data.models
{
    /// <summary>
    /// Model for messages on the timeline
    /// </summary>
    [BsonKnownTypes(typeof(WatchlistFriendsFollowingMessage), 
        typeof(WatchlistFriendsUnfollowingMessage), 
        typeof(WatchlistAddedMessage),
        typeof(WatchlistRemovedMessage))]
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
