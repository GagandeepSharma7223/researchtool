using Microsoft.AspNetCore.Identity;

namespace chapterone.web.managers
{
    public static class AccountManagerErrors
    {
        public static IdentityError ERROR_INVALID_EMAIL = new IdentityError()
        {
            Code = "INVALID_EMAIL",
            Description = "Email address is not valid"
        };

        public static IdentityError ERROR_INVALID_PASSWORD = new IdentityError()
        {
            Code = "INVALID_PASSWORD",
            Description = "Password is not valid"
        };

        public static IdentityError ERROR_ALREADY_REGISTERED = new IdentityError()
        {
            Code = "ALREADY_REGISTERED",
            Description = "User is already registered"
        };

        public static IdentityError ERROR_USER_NOT_FOUND = new IdentityError()
        {
            Code = "USER_NOT_FOUND",
            Description = "Someting went wrong"
        };

        public static IdentityError ERROR_GENERIC = new IdentityError()
        {
            Code = "GENERIC",
            Description = "Someting went wrong"
        };
    }
}
