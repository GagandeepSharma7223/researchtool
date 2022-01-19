using chapterone.services.interfaces;
using chapterone.shared;
using chapterone.web.Areas.Admin.ViewModels;
using chapterone.web.identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace chapterone.web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class RolesController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEventLogger _logger;

        public RolesController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IEventLogger logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.AsEnumerable();
            return View(roles);
        }

        [HttpGet("Create")]
        public IActionResult Create() => View();

        [HttpPost("Create")]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await _roleManager.CreateAsync(new ApplicationRole()
                    {
                        Name = model.Name,
                        Version = Constants.SCHEMAVERSION_USER
                    });

                    if (!result.Succeeded)
                    {
                        foreach (IdentityError error in result.Errors)
                            ModelState.AddModelError("Name", error.Description);
                        return View();
                    }
                }
                return RedirectToAction("");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return NotFound();
            }
        }
    }
}
