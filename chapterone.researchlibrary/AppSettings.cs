using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chapterone.web
{
    public class AppSettings : IAppSettings
    {
        public string Host { get; set; }
        public string SendGridApiKey { get; set; }
    }


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
