using chapterone.services.interfaces;
using chapterone.shared;
using chapterone.shared.models;
using chapterone.web.Areas.Admin.ViewModels;
using chapterone.web.Helper;
using chapterone.web.identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SendGrid;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace chapterone.web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICustomEmailService _customEmailService;
        private readonly IEventLogger _logger;
        private readonly IViewRenderService _viewRenderService;
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IEventLogger logger,
            ICustomEmailService customEmailService, IViewRenderService viewRenderService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _customEmailService = customEmailService;
            _viewRenderService = viewRenderService;
        }

        #region Index
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var users = _userManager.Users.Where(x => x.Id != currentUser.Id).AsEnumerable().Select(async user => await ParseUserAsync(user))
                       .Select(t => t.Result);
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return BadRequest();
            }
        }

        private async Task<UserViewModel> ParseUserAsync(ApplicationUser user)
        {
            var viewModel = new UserViewModel
            {
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed ? "Yes" : "No",
                Id = user.Id,
                Name = user.Name,
                Version = user.Version,
                Roles = await _userManager.GetRolesAsync(user)
            };
            return viewModel;
        }

        #endregion

        #region Create
        [HttpGet("Create")]
        public ViewResult Create() => View(new UserModel());

        [HttpPost("Create")]
        public async Task<IActionResult> Create(UserModel user)
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
                        Version = Constants.SCHEMAVERSION_USER
                    };

                    IdentityResult result = await _userManager.CreateAsync(appUser, Guid.NewGuid().ToString());
                    if (!result.Succeeded)
                    {
                        foreach (IdentityError error in result.Errors)
                            ModelState.AddModelError("Email", error.Description);
                        return View(user);
                    }
                    //Adding User to Admin Role
                    var role = _roleManager.Roles.Where(x => x.Id == user.RoleId).FirstOrDefault();
                    await _userManager.AddToRoleAsync(appUser, role.Name);

                    //Create User Notification Settings
                    string view = "~/Views/EmailTemplates/NewUserAccountEmail.cshtml";
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    string confirmationLink = Url.Action("ConfirmEmail", "Auth", new { token, email = user.Email, Area = "" }, Request.Scheme);

                    EmailViewModel emailViewModel = new EmailViewModel
                    {
                        Name = user.Name,
                        Url = confirmationLink
                    };
                    var html = await _viewRenderService.RenderToStringAsync(view, emailViewModel);
                    await _customEmailService.SendEmailAsync(user.Email, "Confirm User Account", html);
                }
                return RedirectToAction("");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return NotFound();
            }
        }
        #endregion

        #region Delete
        [HttpGet("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return BadRequest();
            }
        }

        [HttpPost("Delete")]
        public async Task<JsonResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                await _userManager.DeleteAsync(user);
                return Json(new { Success = true, Messege = "User Deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return Json(new { Success = false });
            }
        }
        #endregion
    }
}
