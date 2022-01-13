using chapterone.data.interfaces;
using chapterone.data.models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace chapterone.web.identity
{
    /// <summary>
    /// Custom identity user store that uses IDatabaseRepository to store identities
    /// </summary>
    public class UserStore : IUserStore<User>,
                                IUserPasswordStore<User>,
                                IUserSecurityStampStore<User>,
                                IUserPhoneNumberStore<User>,
                                IUserEmailStore<User>,
                                IUserTwoFactorStore<User>,
                                IUserLoginStore<User>,
                                IQueryableUserStore<User>
    //where T : User
    {
        private readonly IdentityErrorDescriber _describer = new IdentityErrorDescriber();
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;

        public IQueryable<User> Users => _userRepo.AsQueryable();


        /// <summary>
        /// Constructor
        /// </summary>
        public UserStore(IUserRepository userRepo, IRoleRepository roleRepository, IUserRoleRepository userRoleRepository)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepository;
            _userRoleRepo = userRoleRepository;
        }

        #region IUserStore

        /// <inheritdoc />
        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            var result = await FindByNameAsync(user.UserName, cancellationToken);

            if (result != null)
                return IdentityResult.Failed(_describer.DuplicateUserName(user.Email));

            await _userRepo.InsertAsync(user);

            return IdentityResult.Success;
        }


        /// <inheritdoc />
        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            await _userRepo.DeleteAsync(user);
            return IdentityResult.Success;
        }


        /// <inheritdoc />
        public void Dispose()
        {
        }


        /// <inheritdoc />
        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await SafeFetch(userId);
        }


        /// <inheritdoc />
        public async Task<User> FindByNameAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            // Note. We search by normalized email instead since we're using 'name' to
            // stash our user's ID (so it is persisted into the _auth token for us)
            var results = await _userRepo.QueryAsync(x => x.NormalizedEmail == normalizedEmail, 1);

            return results.FirstOrDefault();
        }


        /// <inheritdoc />
        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }


        /// <inheritdoc />
        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }


        /// <inheritdoc />
        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }


        /// <inheritdoc />
        public async Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.NormalizedUserName = normalizedName;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public async Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.UserName = userName;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            await _userRepo.UpdateAsync(user);
            return IdentityResult.Success;
        }

        #endregion

        #region IUserPasswordStore

        /// <inheritdoc />
        public async Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.PasswordHash = passwordHash;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }


        /// <inheritdoc />
        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        #endregion

        #region IUserSecurityStampStore

        /// <inheritdoc />
        public async Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.SecurityStamp = stamp;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        #endregion

        #region IUserPhoneNumberStore

        /// <inheritdoc />
        public async Task SetPhoneNumberAsync(User user, string phoneNumber, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.PhoneNumber = phoneNumber;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<string> GetPhoneNumberAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }


        /// <inheritdoc />
        public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }


        /// <inheritdoc />
        public async Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.PhoneNumberConfirmed = confirmed;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }

        #endregion

        #region IUserEmailStore

        /// <inheritdoc />
        public async Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.Email = email;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }


        /// <inheritdoc />
        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }


        /// <inheritdoc />
        public async Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.EmailConfirmed = confirmed;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var results = await _userRepo.QueryAsync(x => x.NormalizedEmail == normalizedEmail, 1);
            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<IdentityRole>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            var userRoles = await _userRoleRepo.QueryAsync(x => x.UserId == user.Id, 1);
            var roles = await _roleRepo.QueryAsync(x => x.Id == userRoles.First().Id, 1);   
            return roles;
        }


        /// <inheritdoc />
        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }


        /// <inheritdoc />
        public async Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.NormalizedEmail = normalizedEmail;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }

        #endregion

        #region IUserTwoFactorStore

        /// <inheritdoc />
        public async Task SetTwoFactorEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.TwoFactorEnabled = enabled;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<bool> GetTwoFactorEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        #endregion

        #region IUserLoginStore

        /// <inheritdoc />
        public Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        public Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
        {
            IList<UserLoginInfo> result = new List<UserLoginInfo>();
            return Task.FromResult(result);
        }


        /// <inheritdoc />
        public Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Ignore repository errors if the T doesn't exist yet
        /// </summary>
        private async Task<User> SafeFetch(User user)
        {
            return await SafeFetch(user.Id);
        }


        /// <summary>
        /// Ignore repository errors if the T doesn't exist yet
        /// </summary>
        private async Task<User> SafeFetch(string id)
        {
            try
            {
                return await _userRepo.GetByIdAsync(id);
            }
            catch (Exception)
            {
            }

            return null;
        }

        #endregion
    }
}
