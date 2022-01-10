using chapterone.data.interfaces;

namespace chapterone.data.mongodb
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
    }

    public class TwitterSettings : ITwitterSettings
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }
}
