using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace chapterone.web.managers
{
    /// <summary>
    /// Interface for an Account Manager
    /// </summary>
    public interface IAccountManager
    {
        /// <summary>
        /// Determines if the system hasn't gone through setup
        /// </summary>
        bool IsSetupRequired { get; }


        /// <summary>
        /// Create a new user
        /// </summary>
        Task<IdentityResult> CreateUserAsync(string email, string password);


        /// <summary>
        /// Change the given user's email address
        /// </summary>
        Task<IdentityResult> ChangePasswordAsync(string email, string currentPassword, string newPassword);


        /// <summary>
        /// Signin the given user
        /// </summary>
        Task<SignInResult> SignInAsync(string email, string password, bool rememberMe = false);


        /// <summary>
        /// Signout the current user
        /// </summary>
        Task SignOutAsync();


        /// <summary>
        /// Get the password reset token for the user with the given email
        /// </summary>
        Task<string> GetPasswordResetToken(string email);


        /// <summary>
        /// Reset the given user's password
        /// </summary>
        Task<IdentityResult> ResetPassword(string email, string token, string password);
    }
}
