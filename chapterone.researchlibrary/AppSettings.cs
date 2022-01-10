namespace chapterone.web
{
    public class AppSettings : IAppSettings
    {
        public string Host { get; set; }
        public string SendGridApiKey { get; set; }
    }
}
