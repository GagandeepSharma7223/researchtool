using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using chapterone.data.interfaces;
using chapterone.data.mongodb;
using chapterone.data.repositories;
using chapterone.services.clients;
using chapterone.services.interfaces;
using chapterone.web.BackgroundServices;
using chapterone.web.identity;
using chapterone.web.logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDb.Bson.NodaTime;
using MongoDB.Bson.Serialization;
using System;

namespace chapterone.web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mongoDbSettings = Configuration.GetSection("Database").Get<DatabaseSettings>();
            var mongoDbIdentityConfiguration = new MongoDbIdentityConfiguration
            {
                MongoDbSettings = new MongoDbSettings
                {
                    ConnectionString = mongoDbSettings.ConnectionString,
                    DatabaseName = mongoDbSettings.Name
                },
                IdentityOptionsAction = options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;

                    // Lockout settings
                    options.Lockout.AllowedForNewUsers = false;

                    // ApplicationUser settings
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                    //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@.-_";
                }
            };
            services.ConfigureMongoDbIdentity<ApplicationUser, ApplicationRole, string>(mongoDbIdentityConfiguration)
                    .AddDefaultTokenProviders();

            //services.AddAuthentication(o =>
            //{
            //    o.DefaultScheme = IdentityConstants.ApplicationScheme;
            //    o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            //})
            //.AddIdentityCookies(o => {  });


            //services.AddIdentityCore<ApplicationUser>()
            //    .AddRoles<ApplicationRole>()
            //    .AddMongoDbStores<ApplicationUser, ApplicationRole, string>(mongoDbSettings.ConnectionString, mongoDbSettings.Name)
            //    .AddSignInManager()
            //    .AddDefaultTokenProviders();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddMongoDbStores<ApplicationUser, ApplicationRole, string>
                (
                    mongoDbSettings.ConnectionString, mongoDbSettings.Name
                ).AddDefaultTokenProviders();
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
            var settings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            
            // Service injection
            services.AddSingleton<IAppSettings>(settings);
            services.AddScoped<ITwitterWatchlistRepository, TwitterWatchlistRepository>();
            services.AddScoped<ITimeLineRepository, TimeLineRepository>();
            services.AddScoped<ITwitterClient, TwitterClient>();

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(2);
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/error";
                options.SlidingExpiration = true;
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            // Background services
            //services.AddHostedService<MigrateProfileService>();
            //services.AddHostedService<MigrateTimelineService>();
            //services.AddHostedService<TwitterMessageService>();
            //services.AddHostedService<TwitterWatchlistService>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
