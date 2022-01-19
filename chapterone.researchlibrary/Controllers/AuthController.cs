using chapterone.services.interfaces;
using chapterone.shared;
using chapterone.shared.models;
using chapterone.web.Helper;
using chapterone.web.identity;
using chapterone.web.viewmodels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace chapterone.web.controllers
{
    public class AuthController : Controller
    {
        private const string HOMEPAGE = "/";
        private readonly IAppSettings _settings;
        private readonly IEventLogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IViewRenderService _viewRenderService;
        private readonly ICustomEmailService _customEmailService;
        private const int SchemaVersion = 6;
        /// <summary>
        /// Constructor
        /// </summary>
        public AuthController(IAppSettings settings, IEventLogger logger, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, ICustomEmailService customEmailService, IViewRenderService viewRenderService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _settings = settings;
            _logger = logger;
            _roleManager = roleManager;
            _customEmailService = customEmailService;
            _viewRenderService = viewRenderService;
        }

        #region Login / Logout

        [HttpGet("login")]
        public IActionResult ViewLogin()
        {
            return View("~/views/Login.cshtml");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([Required][FromForm] string username, [Required][FromForm] string password, [FromQuery] string redirect = HOMEPAGE)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser appUser = await _userManager.FindByEmailAsync(username);
                    if (appUser != null)
                    {
                        await _signInManager.SignOutAsync();
                        var result = await _signInManager.PasswordSignInAsync(appUser, password, false, false);

                        if (result.Succeeded)
                        {
                            var url = string.IsNullOrWhiteSpace(redirect) ? HOMEPAGE : Uri.UnescapeDataString(redirect);
                            return LocalRedirect(url);
                        }
                    }
                    // Post-back for handling errors
                    var fieldErrors = new List<string>();
                    TempData["error_fields"] = fieldErrors;
                    TempData["error_message"] = "Invalid username or password";
                    TempData["postback_username"] = username;
                    return Redirect("/login");
                }
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return NotFound();
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return new RedirectResult("/login");
        }

        #endregion

        #region Forgot password

        [HttpGet("/forgotpassword")]
        public IActionResult ViewForgotPassword()
        {
            return View("~/views/ForgotPassword.cshtml");
        }

        [HttpPost("/forgotpassword")]
        public async Task<IActionResult> ForgotPasswordRequest([Required] string email)
        {
            if (!ModelState.IsValid)
                return View("~/views/ForgotPassword.cshtml");
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return LocalRedirect(ErrorPage.Path);

            string code = await _userManager.GeneratePasswordResetTokenAsync(user);

            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["error_fields"] = new string[] { "email" };
                TempData["error_message"] = "Invalid email address";

                return Redirect("/forgotpassword");
            }
            //Create User Notification Settings
            var emailEncoded = UrlEncoder.Default.Encode(email);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var confirmationLink = $"https://{_settings.Host}/resetpassword?email={emailEncoded}&token={code}";
            string view = "~/Views/EmailTemplates/SendForgotPasswordEmail.cshtml";
            EmailViewModel emailViewModel = new EmailViewModel
            {
                Name = user.Name,
                Url = confirmationLink
            };
            var html = await _viewRenderService.RenderToStringAsync(view, emailViewModel);
            await _customEmailService.SendEmailAsync(user.Email, "🔐 Reset your password", html);
            return Ok("Please Check your email");
        }

        #endregion

        #region Reset password

        [HttpGet("/resetpassword")]
        public IActionResult ViewResetPassword([FromQuery] string email, [FromQuery] string token)
        {
            if (token == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            var viewModel = new SetPasswordViewModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)),
                Email = email
            };
            return View("~/views/ResetPassword.cshtml", viewModel);
        }


        [HttpPost("/resetpassword")]
        public async Task<IActionResult> ResetPasswordRequest(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                ViewBag.Success = true;
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
                return Redirect("/login");
            // Post-back for handling errors
            var fieldErrors = new List<string>() { "password" };
            TempData["error_fields"] = fieldErrors;
            TempData["error_message"] = result.Errors.First().Description;
            return Redirect("/resetpassword");
        }

        #endregion

        #region ConfirmEmail
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return LocalRedirect(ErrorPage.Path);
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    string resetPasswordCode = await _userManager.GeneratePasswordResetTokenAsync(user);
                    resetPasswordCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetPasswordCode));
                    return RedirectToAction("SetPassword", new { code = resetPasswordCode, id = user.Id });
                }
                return LocalRedirect(ErrorPage.Path);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return LocalRedirect(ErrorPage.Path);
            }
        }
        #endregion

        #region SetPassword
        [HttpGet("SetPassword")]
        public async Task<IActionResult> SetPassword(string code, string id)
        {
            try
            {
                if (code == null)
                {
                    return BadRequest("A code must be supplied for password reset.");
                }
                else
                {
                    var user = await _userManager.FindByIdAsync(id);
                    var model = new SetPasswordViewModel
                    {
                        Code = code,
                        Email = user.Email
                    };
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return LocalRedirect(ErrorPage.Path);
            }
        }

        [HttpPost("SetPassword")]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    ViewBag.Success = true;
                    return View(model);
                }

                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
                var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
                if (result.Succeeded)
                {
                    ViewBag.Success = true;
                    // Send new password create email
                    string view = "~/Views/EmailTemplates/PasswordChangeEmail.cshtml";
                    PasswordChangeEmailVM emailViewModel = new PasswordChangeEmailVM
                    {
                        Name = user.Name,
                        Email = user.Email,
                        PasswordChangeTime = DateTime.UtcNow.ToString("dddd, dd MMMM yyyy HH:mm:ss"),
                        IsForNewPasword = true,
                        PasswordResetUrl = $"https://{_settings.Host}/forgotpassword"
                    };
                    var html = await _viewRenderService.RenderToStringAsync(view, emailViewModel);
                    await _customEmailService.SendEmailAsync(user.Email, "New password created for your account", html);
                    return LocalRedirect("~/login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return LocalRedirect(ErrorPage.Path);
            }
        }
        #endregion

    }
}
