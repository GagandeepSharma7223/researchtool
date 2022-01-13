using chapterone.data.enums;
using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.services.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace chapterone.web.BackgroundServices
{
    public class MigrateTimelineService : BackgroundService
    {
        #region Initialize
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _serviceScope;
        private readonly ITimeLineRepository _timelineRepo;
        private readonly ITwitterClient _twitterClient;

        public MigrateTimelineService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceScope = _serviceProvider.CreateScope();
            _timelineRepo = _serviceScope.ServiceProvider.GetService<ITimeLineRepository>();
            _twitterClient = _serviceScope.ServiceProvider.GetService<ITwitterClient>();
        }

        #endregion

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int hours = 6, mins = 15;
                TimeSpan serviceToRunAt = new TimeSpan(hours, mins, 0); //6:15AM
                TimeSpan timeNow = DateTime.Now.TimeOfDay;
                if (timeNow > serviceToRunAt)
                {
                    await RunProcessAsync();
                    //Run this task once every day
                    var now = DateTime.Now;
                    var tomorrow = DateTime.Today.AddDays(1).AddHours(hours).AddMinutes(mins);
                    var timeSpan = tomorrow - now;
                    await Task.Delay(timeSpan, stoppingToken);
                }
                else
                {
                    await Task.Delay(serviceToRunAt - DateTime.Now.TimeOfDay);
                }
            }
        }

        private async Task RunProcessAsync()
        {
            const int TIMELINE_SCHEMAVERSION = 6;

            var messages = await _timelineRepo.QueryAsync(x => x.SchemaVersion != TIMELINE_SCHEMAVERSION);
            foreach (var message in messages)
            {
                switch (message.Type)
                {
                    case MessageType.TwitterWatchlistFriendsFollowed:
                        await MigrateFriendsFollowedMessage(message as WatchlistFriendsFollowingMessage);
                        break;
                    case MessageType.TwitterWatchlistFriendsUnfollowed:
                        await MigrateFriendsUnfollowedMessage(message as WatchlistFriendsUnfollowingMessage);
                        break;
                    case MessageType.TwitterWatchlistProfileAdded:
                        await MigrateFriendAddedMessage(message as WatchlistAddedMessage);
                        break;
                    case MessageType.TwitterWatchlistProfileRemoved:
                        await MigrateFriendRemovedMessage(message as WatchlistRemovedMessage);
                        break;
                }

                message.SchemaVersion = TIMELINE_SCHEMAVERSION;

                await _timelineRepo.UpdateAsync(message);
            }
        }

        private async Task MigrateFriendsFollowedMessage(WatchlistFriendsFollowingMessage message)
        {
            var user = await _twitterClient.GetUserByScreenNameAsync(message.ProfileScreenName);

            message.ProfileAvatarUri = user.ProfileImageUri;

            foreach (var name in message.FollowedScreenNames)
            {
                user = await _twitterClient.GetUserByScreenNameAsync(name.ScreenName);

                name.BannerImageUri = user.BannerImageUri;
                name.AvatarUri = user.ProfileImageUri;
                name.Name = user.Name;
                name.Biography = user.Description;
            }
        }

        private async Task MigrateFriendsUnfollowedMessage(WatchlistFriendsUnfollowingMessage message)
        {
            foreach (var name in message.UnfollowedScreenNames)
            {
                var user = await _twitterClient.GetUserByScreenNameAsync(name.ScreenName);

                name.BannerImageUri = user.BannerImageUri;
                name.AvatarUri = user.ProfileImageUri;
                name.Name = user.Name;
                name.Biography = user.Description;
            }
        }

        private async Task MigrateFriendAddedMessage(WatchlistAddedMessage message)
        {
            foreach (var name in message.AddedScreenNames)
            {
                var user = await _twitterClient.GetUserByScreenNameAsync(name.ScreenName);

                name.BannerImageUri = user.BannerImageUri;
                name.AvatarUri = user.ProfileImageUri;
                name.Name = user.Name;
                name.Biography = user.Description;
            }
        }

        private async Task MigrateFriendRemovedMessage(WatchlistRemovedMessage message)
        {
            foreach (var name in message.RemovedScreenNames)
            {
                var user = await _twitterClient.GetUserByScreenNameAsync(name.ScreenName);

                name.BannerImageUri = user.BannerImageUri;
                name.AvatarUri = user.ProfileImageUri;
                name.Name = user.Name;
                name.Biography = user.Description;
            }
        }

    }
}
