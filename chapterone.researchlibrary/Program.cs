using chapterone.shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SendGrid.Extensions.DependencyInjection;

namespace chapterone.web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices((context, services) => services.AddSendGrid(options =>
                    options.ApiKey = context.Configuration.GetValue<string>(ConfigurationKeys.SendGrid__Key)
                ));
    }
}
