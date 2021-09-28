using chapterone.data.models;
using System.Collections.Generic;

namespace chapterone.web.viewmodels
{
    public class WatchlistViewModel
    {
        public IEnumerable<ProfileViewModel> Profiles { get; set; } = new List<ProfileViewModel>();
    }

    public class ProfileViewModel
    {
        public string AvatarUri { get; set; }
        public string BannerUri { get; set; }
        public string ScreenName { get; set; }
        public string Name { get; set; }
        public string Biography { get; set; }
        public long NumberOfFriends { get; set; }
        public string WatchingSince { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileViewModel(TwitterWatchlistProfile profile)
        {
            AvatarUri = profile.ProfileImageUri;
            BannerUri = profile.BannerImageUri;
            ScreenName = profile.ScreenName;
            Name = profile.Name;
            Biography = profile.Biography;
            NumberOfFriends = profile.FriendIds.LongLength;
            WatchingSince = profile.Created.ToString("dd MMM uuuu", null);
        }
    }
}
