using chapterone.data.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace chapterone.web.identity
{
    /// <summary>
    /// Extension methods to add identity components to the ASP.NET Service collection
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Add support for ASP.NET Core Identity
        /// </summary>
        public static void AddUserIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                    options.Lockout.AllowedForNewUsers = false;

                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;
                })
                .AddUserStore<UserStore>()
                .AddRoleStore<NoOpRoleStore>()
                .AddDefaultTokenProviders();
        }
    }
}
