using chapterone.services.clients;
using System;
using System.Collections.Generic;
using System.Text;
using Tweetinvi.Models;

namespace chapterone.services.extensions
{
    public static class TweetinviExtensions
    {
        public static TwitterUser ToTwitterUser(this IUser thisUser)
        {
            return new TwitterUser()
            {
                UserId = thisUser.Id,
                BannerImageUri = thisUser.ProfileBannerURL,
                ProfileImageUri = thisUser.ProfileImageUrl400x400,
                ScreenName = thisUser.ScreenName,
                Name = thisUser.Name,
                Description = thisUser.Description,
                IsFollowing = thisUser.Following.GetValueOrDefault()
            };
        }
    }
}
