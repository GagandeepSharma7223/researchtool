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
}
