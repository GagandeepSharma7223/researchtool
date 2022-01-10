using chapterone.data.models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace chapterone.web.managers
{
    /// <summary>
    /// Account manager that uses the ASP.NET Identity classes to manage users and signin
    /// </summary>
    public class AccountManager : IAccountManager
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;


        public bool IsSetupRequired => _userManager.Users.Count() == 0;


        /// <summary>
        /// Constructor
        /// </summary>
        public AccountManager(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public async Task<IdentityResult> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email)) return IdentityResult.Failed(AccountManagerErrors.ERROR_INVALID_EMAIL);
            if (string.IsNullOrWhiteSpace(currentPassword)) return IdentityResult.Failed(AccountManagerErrors.ERROR_INVALID_PASSWORD);
            if (string.IsNullOrWhiteSpace(newPassword)) return IdentityResult.Failed(AccountManagerErrors.ERROR_INVALID_PASSWORD); // TODO: Return better error

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return IdentityResult.Failed(AccountManagerErrors.ERROR_USER_NOT_FOUND);

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
                return IdentityResult.Failed(result.Errors.FirstOrDefault() ?? AccountManagerErrors.ERROR_GENERIC);

            return IdentityResult.Success;
        }


        public async Task<IdentityResult> CreateUserAsync(string email, string password)
        {
            var user = new User()
            {
                Id = email,
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            // Create a new user
            var result = await _userManager.CreateAsync(user, password);

            // Handle errors
            if (!result.Succeeded)
            {
                return result.Errors.Count() > 0 ? IdentityResult.Failed(AccountManagerErrors.ERROR_ALREADY_REGISTERED) :
                    IdentityResult.Failed(AccountManagerErrors.ERROR_GENERIC);
            }

            return IdentityResult.Success;
        }


        public Task<SignInResult> SignInAsync(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(email)) return Task.FromResult(SignInResult.Failed);
            if (string.IsNullOrWhiteSpace(password)) return Task.FromResult(SignInResult.Failed);

            return _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        }


        public Task SignOutAsync()
        {
            return _signInManager.SignOutAsync();
        }


        public async Task<string> GetPasswordResetToken(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));

            var user = await _userManager.FindByEmailAsync(email);

            return user != null ? await _userManager.GeneratePasswordResetTokenAsync(user) : null;
        }


        public async Task<IdentityResult> ResetPassword(string email, string token, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return IdentityResult.Failed(AccountManagerErrors.ERROR_USER_NOT_FOUND);

            return await _userManager.ResetPasswordAsync(user, token, password);
        }
    }
}
