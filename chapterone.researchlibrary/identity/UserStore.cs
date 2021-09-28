using chapterone.data.interfaces;
using chapterone.data.shared;
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
    public class UserStore<T> : IUserStore<T>,
                                IUserPasswordStore<T>,
                                IUserSecurityStampStore<T>,
                                IUserPhoneNumberStore<T>,
                                IUserEmailStore<T>,
                                IUserTwoFactorStore<T>,
                                IUserLoginStore<T>,
                                IQueryableUserStore<T>
        where T : IdentityUser<string>, IEntity
    {
        private readonly IdentityErrorDescriber _describer = new IdentityErrorDescriber();
        private readonly IDatabaseRepository<T> _userRepo;

        public IQueryable<T> Users => _userRepo.AsQueryable();


        /// <summary>
        /// Constructor
        /// </summary>
        public UserStore(IDatabaseRepository<T> userRepo)
        {
            _userRepo = userRepo;
        }

        #region IUserStore

        /// <inheritdoc />
        public async Task<IdentityResult> CreateAsync(T user, CancellationToken cancellationToken)
        {
            var result = await FindByNameAsync(user.UserName, cancellationToken);

            if (result != null)
                return IdentityResult.Failed(_describer.DuplicateUserName(user.Email));

            await _userRepo.InsertAsync(user);

            return IdentityResult.Success;
        }


        /// <inheritdoc />
        public async Task<IdentityResult> DeleteAsync(T user, CancellationToken cancellationToken)
        {
            await _userRepo.DeleteAsync(user);
            return IdentityResult.Success;
        }


        /// <inheritdoc />
        public void Dispose()
        {
        }


        /// <inheritdoc />
        public async Task<T> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await SafeFetch(userId);
        }


        /// <inheritdoc />
        public async Task<T> FindByNameAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            // Note. We search by normalized email instead since we're using 'name' to
            // stash our user's ID (so it is persisted into the _auth token for us)
            var results = await _userRepo.QueryAsync(x => x.NormalizedEmail == normalizedEmail, 1);

            return results.FirstOrDefault();
        }


        /// <inheritdoc />
        public Task<string> GetNormalizedUserNameAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }


        /// <inheritdoc />
        public Task<string> GetUserIdAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }


        /// <inheritdoc />
        public Task<string> GetUserNameAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }


        /// <inheritdoc />
        public async Task SetNormalizedUserNameAsync(T user, string normalizedName, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.NormalizedUserName = normalizedName;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public async Task SetUserNameAsync(T user, string userName, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.UserName = userName;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public async Task<IdentityResult> UpdateAsync(T user, CancellationToken cancellationToken)
        {
            await _userRepo.UpdateAsync(user);
            return IdentityResult.Success;
        }

        #endregion

        #region IUserPasswordStore

        /// <inheritdoc />
        public async Task SetPasswordHashAsync(T user, string passwordHash, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.PasswordHash = passwordHash;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<string> GetPasswordHashAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }


        /// <inheritdoc />
        public Task<bool> HasPasswordAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        #endregion

        #region IUserSecurityStampStore

        /// <inheritdoc />
        public async Task SetSecurityStampAsync(T user, string stamp, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.SecurityStamp = stamp;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<string> GetSecurityStampAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        #endregion

        #region IUserPhoneNumberStore

        /// <inheritdoc />
        public async Task SetPhoneNumberAsync(T user, string phoneNumber, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.PhoneNumber = phoneNumber;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<string> GetPhoneNumberAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }


        /// <inheritdoc />
        public Task<bool> GetPhoneNumberConfirmedAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }


        /// <inheritdoc />
        public async Task SetPhoneNumberConfirmedAsync(T user, bool confirmed, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.PhoneNumberConfirmed = confirmed;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }

        #endregion

        #region IUserEmailStore

        /// <inheritdoc />
        public async Task SetEmailAsync(T user, string email, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.Email = email;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<string> GetEmailAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }


        /// <inheritdoc />
        public Task<bool> GetEmailConfirmedAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }


        /// <inheritdoc />
        public async Task SetEmailConfirmedAsync(T user, bool confirmed, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.EmailConfirmed = confirmed;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public async Task<T> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var results = await _userRepo.QueryAsync(x => x.NormalizedEmail == normalizedEmail, 1);
            return results.FirstOrDefault();
        }


        /// <inheritdoc />
        public Task<string> GetNormalizedEmailAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }


        /// <inheritdoc />
        public async Task SetNormalizedEmailAsync(T user, string normalizedEmail, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.NormalizedEmail = normalizedEmail;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }

        #endregion

        #region IUserTwoFactorStore

        /// <inheritdoc />
        public async Task SetTwoFactorEnabledAsync(T user, bool enabled, CancellationToken cancellationToken)
        {
            var result = await SafeFetch(user.Id);
            user.TwoFactorEnabled = enabled;

            if (result != null)
                await _userRepo.UpdateAsync(user);
        }


        /// <inheritdoc />
        public Task<bool> GetTwoFactorEnabledAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        #endregion

        #region IUserLoginStore

        /// <inheritdoc />
        public Task AddLoginAsync(T user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        public Task RemoveLoginAsync(T user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        public Task<IList<UserLoginInfo>> GetLoginsAsync(T user, CancellationToken cancellationToken)
        {
            IList<UserLoginInfo> result = new List<UserLoginInfo>();
            return Task.FromResult(result);
        }


        /// <inheritdoc />
        public Task<T> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Ignore repository errors if the T doesn't exist yet
        /// </summary>
        private async Task<T> SafeFetch(T user)
        {
            return await SafeFetch(user.Id);
        }


        /// <summary>
        /// Ignore repository errors if the T doesn't exist yet
        /// </summary>
        private async Task<T> SafeFetch(string id)
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
