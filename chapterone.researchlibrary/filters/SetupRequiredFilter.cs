using chapterone.web.managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace chapterone.web.filters
{
    public class SetupRequiredFilter : IAuthorizationFilter
    {
        /// <summary>
        /// The path to redirect to if the system is not setup
        /// </summary>
        public string SetupPath { get; set; }


        /// <summary>
        /// On authorisation
        /// </summary>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var service = context.HttpContext.RequestServices.GetService(typeof(IAccountManager)) as IAccountManager;
            
            if (service.IsSetupRequired && !context.HttpContext.Request.Path.Equals(SetupPath, StringComparison.InvariantCultureIgnoreCase))
            {
                // System needs to be set up...
                context.Result = new RedirectResult(SetupPath);
            }
        }
    }
}
