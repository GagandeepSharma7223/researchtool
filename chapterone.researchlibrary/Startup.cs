using chapterone.data.enums;
using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.data.mongodb;
using chapterone.data.repositories;
using chapterone.services.clients;
using chapterone.services.interfaces;
using chapterone.services.scheduling;
using chapterone.shared;
using chapterone.web.BackgroundServices;
using chapterone.web.filters;
using chapterone.web.identity;
using chapterone.web.logging;
using chapterone.web.managers;
using chapterone.web.middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDb.Bson.NodaTime;
using MongoDB.Bson.Serialization;
using System;
using System.Threading.Tasks;

namespace chapterone.web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            services.AddSingleton<IAppSettings>(settings);
            services.AddScoped<ITwitterWatchlistRepository, TwitterWatchlistRepository>();
            services.AddScoped<ITimeLineRepository, TimeLineRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITwitterClient, TwitterClient>();

            InitialiseServices(services, settings);
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
                    var userRepo = context.HttpContext.RequestServices.GetService<IUserRepository>();

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
            var builder = services.AddControllersWithViews(config =>
            {
                config.Filters.Add(new SetupRequiredFilter()
                {
                    SetupPath = "/setup"
                });
                config.Filters.Add(new AccessAuthFilter()
                {
                    LoginPath = "/login"
                });
            });
            services.AddRazorPages();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
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
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Admin}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Tenants}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Landlords}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// Initialise all the core services
        /// </summary>
        private void InitialiseServices(IServiceCollection services, IAppSettings settings)
        {
            // App Insight settings
            var appInsightsKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"] ?? string.Empty;
            var logger = new AppInsightsEventLogger(appInsightsKey);
            services.AddSingleton<IEventLogger>(logger);
            services.AddApplicationInsightsTelemetry(appInsightsKey);

            // Add services to the container.
            services.Configure<DatabaseSettings>(Configuration.GetSection("Database"));
            services.Configure<TwitterSettings>(Configuration.GetSection("Twitter"));
            services.AddSingleton<IDatabaseSettings>(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value);
            services.AddSingleton<ITwitterSettings>(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<TwitterSettings>>().Value);
            BsonSerializer.RegisterSerializer(new ZonedDateTimeSerializer());

            //var emailService = new SendGridEmailService(settings.SendGridApiKey);
                      
            // Background services
            services.AddHostedService<MigrateProfileService>();
            services.AddHostedService<MigrateTimelineService>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, SchedulerHostedService>(serviceProvider =>
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

            services.AddUserIdentity();
        }

    }
}
