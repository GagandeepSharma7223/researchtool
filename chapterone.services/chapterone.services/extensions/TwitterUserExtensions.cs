using chapterone.data.models;
using chapterone.services.interfaces;

namespace chapterone.services.extensions
{
    public static class TwitterUserExtensions
    {
        public static TwitterScreenName ToTwitterScreenName(this ITwitterUser user)
        {
            return new TwitterScreenName()
            {
                BannerImageUri = user.BannerImageUri,
                AvatarUri = user.ProfileImageUri,
                ScreenName = user.ScreenName,
                Name = user.Name,
                Biography = string.IsNullOrWhiteSpace(user.Description) ? "No biography" : user.Description,
                IsFriend = user.IsFollowing
            };
        }
    }
}
