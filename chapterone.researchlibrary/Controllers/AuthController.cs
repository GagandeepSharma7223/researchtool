using chapterone.services.interfaces;
using chapterone.shared.models;
using chapterone.web.identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace chapterone.web.controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        private const string HOMEPAGE = "/";
        private readonly IAppSettings _settings;
        private readonly IEventLogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private const int SchemaVersion = 6;
        /// <summary>
        /// Constructor
        /// </summary>
        public AuthController(IAppSettings settings, IEventLogger logger, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _settings = settings;
            _logger = logger;
            _roleManager = roleManager;
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
            //var token = await  _accountManager.GetPasswordResetToken(email);

            //if (string.IsNullOrWhiteSpace(token))
            //{
            //    TempData["error_fields"] = new string[] { "email" };
            //    TempData["error_message"] = "Invalid email address";

            //    return Redirect("/forgotpassword");
            //}

            //var emailEncoded = UrlEncoder.Default.Encode(email);
            //var tokenEncoded = UrlEncoder.Default.Encode(token);
            //var confirmationLink = $"https://{_settings.Host}/resetpassword?email={emailEncoded}&token={tokenEncoded}";

            //var emailHtml = "Hello!<br/><br/>" +
            //    "Reset your password by clicking on this link:<br/><br/>" +
            //   $"<a style=\"background-color: #00babe; border-radius: 5px; text-align: center; padding: 0.5em 2em; text-decoration: none; color: white;\" href=\"{confirmationLink}\">Reset password</a><br/><br/>" +
            //    "";

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
            //var result = await _accountManager.ResetPassword(email, token, password);

            //if (!result.Succeeded)
            //{
            //    // Post-back for handling errors
            //    var fieldErrors = new List<string>() { "password" };

            //    TempData["error_fields"] = fieldErrors;
            //    TempData["error_message"] = result.Errors.First().Description;

            //    return Redirect("/resetpassword");
            //}

            return Redirect("/login");
        }

        #endregion

        #region Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Users()
        {
            var users = _userManager.Users.AsEnumerable();
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public ViewResult CreateUser() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(UserModel user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser appUser = new ApplicationUser
                    {
                        UserName = user.Email,
                        Email = user.Email,
                        Name = user.Name,
                        Version = SchemaVersion
                    };

                    IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);
                    if (result.Succeeded)
                        ViewBag.Message = "User Created Successfully";
                    else
                    {
                        foreach (IdentityError error in result.Errors)
                            ModelState.AddModelError("", error.Description);
                    }
                    //Adding User to Admin Role
                    var role = _roleManager.Roles.Where(x => x.Id == user.RoleId).FirstOrDefault();
                    await _userManager.AddToRoleAsync(appUser, role.Name);
                }
                return RedirectToAction("Users");   
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return NotFound();
            }
        } 
        #endregion

        #region Roles

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Roles()
        {
            var roles = _roleManager.Roles.AsEnumerable();
            return View(roles);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateRole() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole([Required] string name)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await _roleManager.CreateAsync(new ApplicationRole()
                    {
                        Name = name,
                        Version = SchemaVersion
                    });
                    if (result.Succeeded)
                        ViewBag.Message = "Role Created Successfully";
                    else
                    {
                        foreach (IdentityError error in result.Errors)
                            ModelState.AddModelError("", error.Description);
                    }
                }
                return RedirectToAction("Roles");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return NotFound();
            }
        }
        #endregion
    }
}
