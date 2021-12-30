using chapterone.email;
using chapterone.services.interfaces;
using chapterone.web.managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace chapterone.web.controllers
{
    public class AuthController : Controller
    {
        private const string CLAIMTYPE_USERID = "uid";
        private const string HOMEPAGE = "/";

        private const int LOGIN_EXPIRATION_DAYS = 1;

        private readonly IAccountManager _accountManager;
        //private readonly IEmailService _emailService;
        private readonly IAppSettings _settings;
        private readonly IEventLogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public AuthController(IAccountManager accountManager, IAppSettings settings, IEventLogger logger)
        {
            _accountManager = accountManager;
            //_emailService = emailService;
            _settings = settings;
            _logger = logger;
        }

        #region Login / Logout

        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult ViewLogin()
        {
            return View("~/views/Login.cshtml");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password, [FromQuery] string redirect = HOMEPAGE)
        {
            var result = await _accountManager.SignInAsync(username, password);

            if (!result.Succeeded)
            {
                // Post-back for handling errors
                var fieldErrors = new List<string>();

                TempData["error_fields"] = fieldErrors;
                TempData["error_message"] = "Invalid username or password";
                TempData["postback_username"] = username;

                return Redirect("/login");
            }

            var url = string.IsNullOrWhiteSpace(redirect) ? HOMEPAGE : Uri.UnescapeDataString(redirect);
            return LocalRedirect(url);
        }


        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _accountManager.SignOutAsync();

            return new RedirectResult("/login");
        }

        #endregion

        #region Forgot password

        [AllowAnonymous]
        [HttpGet("/forgotpassword")]
        public IActionResult ViewForgotPassword()
        {
            return View("~/views/ForgotPassword.cshtml");
        }


        [AllowAnonymous]
        [HttpPost("/forgotpassword")]
        public async Task<IActionResult> ForgotPasswordRequest(string email)
        {
            var token = await _accountManager.GetPasswordResetToken(email);

            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["error_fields"] = new string[] { "email" };
                TempData["error_message"] = "Invalid email address";

                return Redirect("/forgotpassword");
            }

            var emailEncoded = UrlEncoder.Default.Encode(email);
            var tokenEncoded = UrlEncoder.Default.Encode(token);
            var confirmationLink = $"https://{_settings.Host}/resetpassword?email={emailEncoded}&token={tokenEncoded}";

            var emailHtml = "Hello!<br/><br/>" +
                "Reset your password by clicking on this link:<br/><br/>" +
               $"<a style=\"background-color: #00babe; border-radius: 5px; text-align: center; padding: 0.5em 2em; text-decoration: none; color: white;\" href=\"{confirmationLink}\">Reset password</a><br/><br/>" +
                "";

            // TBD sendgrid is not setup to send email
            //if (!await _emailService.SendEmail(email, "🔐 Reset your password", emailHtml))
            //    return BadRequest();

            return Ok("Check your email");
        }

        #endregion

        #region Reset password

        [AllowAnonymous]
        [HttpGet("/resetpassword")]
        public IActionResult ViewResetPassword([FromQuery] string email, [FromQuery] string token)
        {
            TempData["postback_username"] = email;
            TempData["postback_token"] = token;

            return View("~/views/ResetPassword.cshtml");
        }


        [AllowAnonymous]
        [HttpPost("/resetpassword")]
        public async Task<IActionResult> ResetPasswordRequest(string email, string token, string password)
        {
            var result = await _accountManager.ResetPassword(email, token, password);

            if (!result.Succeeded)
            {
                // Post-back for handling errors
                var fieldErrors = new List<string>() { "password" };

                TempData["error_fields"] = fieldErrors;
                TempData["error_message"] = result.Errors.First().Description;

                return Redirect("/resetpassword");
            }

            return Redirect("/login");
        }

        #endregion
    }
}
