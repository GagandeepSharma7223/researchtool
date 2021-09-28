using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.models.twitter;
using chapterone.data.mongodb;
using chapterone.email;
using chapterone.email.sendgrid;
using chapterone.logic.interfaces;
using chapterone.logic.services;
using chapterone.services.clients;
using chapterone.services.interfaces;
using chapterone.services.scheduling;
using chapterone.shared;
using chapterone.web.filters;
using chapterone.web.identity;
using chapterone.web.logging;
using chapterone.web.managers;
using chapterone.web.middlewares;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Threading.Tasks;

namespace chapterone.web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            services.AddSingleton<IAppSettings>(settings);

            InitialiseServices(services, settings).Wait();

            services.AddUserIdentity();
            services.AddScoped<IAccountManager, AccountManager>();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Domain = new Uri($"https://{settings.Host}").Host;
                options.Cookie.HttpOnly = false;
                options.Cookie.Name = Constants.AUTH_COOKIE_NAME;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                options.Cookie.Path = "/";

                options.LoginPath = "/login";
                options.LogoutPath = "/logout";

                options.Events.OnValidatePrincipal = async context =>
                {
                    var userRepo = context.HttpContext.RequestServices.GetService<IDatabaseRepository<User>>();

                    var userId = context.Principal.UserId();
                    var securityStamp = context.Principal.SecurityStamp();

                    var users = await userRepo.QueryAsync(x => x.Id == userId && x.SecurityStamp == securityStamp);

                    // Clear cookie and reject principal if principal was tampered
                    if (users.Count == 0)
                    {
                        context.RejectPrincipal();
                        context.HttpContext.Response.Cookies.Delete(Constants.AUTH_COOKIE_NAME);
                    }
                };

                options.Validate();
            });

            services.AddAuthentication();

            services.AddMvc(options =>
            {
                options.Filters.Add(new SetupRequiredFilter()
                {
                    SetupPath = "/setup"
                });
                options.Filters.Add(new AccessAuthFilter()
                {
                    LoginPath = "/login"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<EnsureHttpsMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc();
        }


        /// <summary>
        /// Initialise all the core services
        /// </summary>
        private async Task InitialiseServices(IServiceCollection services, IAppSettings settings)
        {
            var appInsightsKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"] ?? string.Empty;
            var logger = new AppInsightsEventLogger(appInsightsKey);
            services.AddSingleton<IEventLogger>(logger);
            services.AddApplicationInsightsTelemetry(appInsightsKey);

            var databaseSettings = Configuration.GetSection("Database").Get<DatabaseSettings>();
            var twitterSettings = Configuration.GetSection("Twitter").Get<TwitterSettings>();

            var config = new MongoDbConfig(databaseSettings.ConnectionString, databaseSettings.Name);

            MongoDbSetup.Setup();

            var emailService = new SendGridEmailService(settings.SendGridApiKey);
            var twitterClient = new TwitterClient(twitterSettings.ConsumerKey, twitterSettings.ConsumerSecret, twitterSettings.AccessToken, twitterSettings.AccessTokenSecret);

            var twitterWatchlistRepo = await MongoDbRepository<TwitterWatchlistProfile>.CreateRepository(config, 6, logger, collectionName: "TwitterWatchlist");
            var timelineRepo = await MongoDbRepository<Message>.CreateRepository(config, 6, logger, collectionName: "Timeline");
            var userRepo = await MongoDbRepository<User>.CreateRepository(config, 1, logger, collectionName: "User");

            services.AddSingleton(twitterWatchlistRepo);
            services.AddSingleton(timelineRepo);
            services.AddSingleton(userRepo);

            await MigrateProfilesAsync(twitterWatchlistRepo, twitterClient);
            await MigrateTimelineAsync(timelineRepo, twitterClient);

            var timelineService = new TimelineService(timelineRepo);

            services.AddSingleton<IEmailService>(emailService);
            services.AddSingleton<ITimelineService>(timelineService);
            services.AddSingleton<ITwitterClient>(twitterClient);

            var watchlistMonitor = new TwitterWatchlistMonitor(twitterWatchlistRepo, timelineService, twitterClient, logger);
            var friendlistMonitor = new TwitterWatchlistFriendsMonitor(twitterWatchlistRepo, timelineService, twitterClient, logger);

            services.AddSingleton<IScheduledTask>(watchlistMonitor);
            services.AddSingleton<IScheduledTask>(friendlistMonitor);

            services.AddSingleton<IHostedService, SchedulerHostedService>(serviceProvider =>
            {
                var instance = new SchedulerHostedService(serviceProvider.GetServices<IScheduledTask>(), logger);
                instance.UnobservedTaskException += (sender, args) =>
                {
                    Console.Write(args.Exception.Message);
                    logger.LogException(args.Exception);
                    args.SetObserved();
                };
                return instance;
            });
        }


        private async Task MigrateProfilesAsync(data.interfaces.IDatabaseRepository<TwitterWatchlistProfile> watchlistRepo, ITwitterClient client)
        {
            const int WATCHLIST_SCHEMAVERSION = 6;

            var profiles = await watchlistRepo.QueryAsync(x => x.SchemaVersion != WATCHLIST_SCHEMAVERSION);

            foreach (var profile in profiles)
            {
                var user = await client.GetUserByScreenNameAsync(profile.ScreenName);

                profile.BannerImageUri = user.BannerImageUri;
                profile.ProfileImageUri = user.ProfileImageUri;
                profile.Name = user.Name;
                profile.Biography = user.Description;

                profile.SchemaVersion = WATCHLIST_SCHEMAVERSION;

                await watchlistRepo.UpdateAsync(profile);
            }
        }


        private async Task MigrateTimelineAsync(data.interfaces.IDatabaseRepository<Message> timelineRepo, ITwitterClient client)
        {
            const int TIMELINE_SCHEMAVERSION = 6;

            var messages = await timelineRepo.QueryAsync(x => x.SchemaVersion != TIMELINE_SCHEMAVERSION);
            foreach (var message in messages)
            {
                switch(message.Type)
                {
                    case data.models.enums.MessageType.TwitterWatchlistFriendsFollowed:
                        await MigrateFriendsFollowedMessage(message as WatchlistFriendsFollowingMessage, client);
                        break;
                    case data.models.enums.MessageType.TwitterWatchlistFriendsUnfollowed:
                        await MigrateFriendsUnfollowedMessage(message as WatchlistFriendsUnfollowingMessage, client);
                        break;
                    case data.models.enums.MessageType.TwitterWatchlistProfileAdded:
                        await MigrateFriendAddedMessage(message as WatchlistAddedMessage, client);
                        break;
                    case data.models.enums.MessageType.TwitterWatchlistProfileRemoved:
                        await MigrateFriendRemovedMessage(message as WatchlistRemovedMessage, client);
                        break;
                }

                message.SchemaVersion = TIMELINE_SCHEMAVERSION;

                await timelineRepo.UpdateAsync(message);
            }
        }


        private async Task MigrateFriendsFollowedMessage(WatchlistFriendsFollowingMessage message, ITwitterClient client)
        {
            var user = await client.GetUserByScreenNameAsync(message.ProfileScreenName);

            message.ProfileAvatarUri = user.ProfileImageUri;

            foreach (var name in message.FollowedScreenNames)
            {
                user = await client.GetUserByScreenNameAsync(name.ScreenName);

                name.BannerImageUri = user.BannerImageUri;
                name.AvatarUri = user.ProfileImageUri;
                name.Name = user.Name;
                name.Biography = user.Description;
            }
        }

        private async Task MigrateFriendsUnfollowedMessage(WatchlistFriendsUnfollowingMessage message, ITwitterClient client)
        {
            foreach (var name in message.UnfollowedScreenNames)
            {
                var user = await client.GetUserByScreenNameAsync(name.ScreenName);

                name.BannerImageUri = user.BannerImageUri;
                name.AvatarUri = user.ProfileImageUri;
                name.Name = user.Name;
                name.Biography = user.Description;
            }
        }

        private async Task MigrateFriendAddedMessage(WatchlistAddedMessage message, ITwitterClient client)
        {
            foreach (var name in message.AddedScreenNames)
            {
                var user = await client.GetUserByScreenNameAsync(name.ScreenName);

                name.BannerImageUri = user.BannerImageUri;
                name.AvatarUri = user.ProfileImageUri;
                name.Name = user.Name;
                name.Biography = user.Description;
            }
        }

        private async Task MigrateFriendRemovedMessage(WatchlistRemovedMessage message, ITwitterClient client)
        {
            foreach (var name in message.RemovedScreenNames)
            {
                var user = await client.GetUserByScreenNameAsync(name.ScreenName);

                name.BannerImageUri = user.BannerImageUri;
                name.AvatarUri = user.ProfileImageUri;
                name.Name = user.Name;
                name.Biography = user.Description;
            }
        }
    }
}
