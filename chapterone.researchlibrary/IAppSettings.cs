namespace chapterone.web
{
    public interface IAppSettings
    {
        string Host { get; }
        string SendGridApiKey { get; }
    }
}
