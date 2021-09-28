using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace chapterone.web.middlewares
{
    /// <summary>
    /// Middleware for redirecting HTTP requests to HTTPS
    /// </summary>
    public class EnsureHttpsMiddleware
    {
        private readonly RequestDelegate _next;


        /// <summary>
        /// Constructor
        /// </summary>
        public EnsureHttpsMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        /// <summary>
        /// Middleware invocation
        /// </summary>
        public Task Invoke(HttpContext context)
        {
            if (context.Request.IsHttps)
                return _next(context);
            
            context.Response.Redirect($"https://{context.Request.Host}/");
            return Task.CompletedTask;
        }
    }
}
