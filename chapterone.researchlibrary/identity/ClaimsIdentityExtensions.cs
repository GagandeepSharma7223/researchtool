using System.Linq;
using System.Security.Claims;

namespace chapterone.web.identity
{
    public static class ClaimsIdentityExtensions
    {
        public static string UserId(this ClaimsPrincipal item)
        {
            return item.Identity.Name;
        }

        public static string SecurityStamp(this ClaimsPrincipal item)
        {
            return item.Claims.First(x => x.Type == "AspNet.Identity.SecurityStamp").Value;
        }
    }
}
