namespace chapterone.shared
{
    public static class Constants
    {
        public const string AUTH_COOKIE_NAME = "_auth";

        public const int SCHEMAVERSION_USER = 1;
        public const int SCHEMAVERSION_WATCHLIST = 6;
        public const int SCHEMAVERSION_TIMELINE = 6;
    }

    public static class EventConstants
    {
        public const int HEARTBEAT_INTERVAL_MS = 10000;

        // Events names

        public const string EVENT_WATCHLIST_MONITOR = "TwitterWatchlistMonitor";
        public const string EVENT_FRIENDLIST_MONITOR = "TwitterFriendlistMonitor";
    }

    public class ConfigurationKeys
    {
        public const string SendGrid__User = "SendGrid:User";
        public const string SendGrid__Key = "SendGrid:Key";
        public const string SendGrid__Email = "SendGrid:Email";
    }

    public class ErrorPage
    {
        public const string Path = "~/Error";
    }

    public static class ModelValidation
    {
        public const string PasswordRegEx = @"^(?=.{8,}$)(?=.*?[a-z])(?=.*?[A-Z]).*$";
        public const string PasswordMessage = "Minimum 8 Characters, 1 Lowercase, 1 Uppercase";
    }
}
