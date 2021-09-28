using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace chapterone.web.filters
{
    public class AccessAuthFilter : IAuthorizationFilter
    {
        private static Type _allowAnonymousType = typeof(AllowAnonymousAttribute);


        /// <summary>
        /// The path to redirect to if the admin is not logged in
        /// </summary>
        public string LoginPath { get; set; }


        /// <summary>
        /// On authorization delegate
        /// </summary>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (descriptor == null)
                throw new InvalidOperationException("ActionDescriptor cast has failed");

            if (IsAnonymousAction(descriptor))
                return;

            if (IsAuthenticated(context.HttpContext))
                return;

            // User is accessing an action that needs authentication
            context.Result = new RedirectResult(LoginPath);
        }


        /// <summary>
        /// Determines if the given action allows anonymous requests
        /// </summary>
        private bool IsAnonymousAction(ControllerActionDescriptor actionDescriptor)
        {
            var isAnonController = actionDescriptor.ControllerTypeInfo.CustomAttributes.Any(x => x.AttributeType.Equals(_allowAnonymousType));
            var isAnonMethod = actionDescriptor.MethodInfo.CustomAttributes.Any(x => x.AttributeType.Equals(_allowAnonymousType));

            return isAnonController || isAnonMethod;
        }


        /// <summary>
        /// Determines if a user is currently logged in
        /// </summary>
        private bool IsAuthenticated(HttpContext context)
        {
            return context.User.Identity.IsAuthenticated;
        }
    }
}
