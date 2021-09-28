using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chapterone.web
{
    public interface IAppSettings
    {
        string Host { get; }
        string SendGridApiKey { get; }
    }


    public interface IDatabaseSettings
    {
        string ConnectionString { get; set; }
        string Name { get; set; }
    }

    public interface ITwitterSettings
    {
        string ConsumerKey { get; set; }
        string ConsumerSecret { get; set; }
        string AccessToken { get; set; }
        string AccessTokenSecret { get; set; }
    }
}
