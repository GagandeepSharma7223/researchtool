namespace chapterone.data.interfaces
{
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
