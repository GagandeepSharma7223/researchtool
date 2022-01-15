using chapterone.services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace chapterone.web.controllers
{
    /// <summary>
    /// Controller for handling system setup operations
    /// </summary>
    public class SetupController : Controller
    {
        private readonly IEventLogger _logger;
        /// <summary>
        /// Constructor
        /// </summary>
        public SetupController(IEventLogger logger)
        {
            _logger = logger;
        }


        [HttpGet("setup")]
        [AllowAnonymous]
        public IActionResult SetupView()
        {
            try
            {
                //if (!_accountManager.IsSetupRequired)
                //    return NotFound();

                return View("~/views/Setup.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return NotFound();
            }
        }


        //[HttpPost("setup")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Setup([FromForm] string username, [FromForm] string password)
        //{
        //    try
        //    {
        //        if (!_accountManager.IsSetupRequired)
        //            return NotFound();

        //        var result = await _accountManager.CreateUserAsync(username, password);
        //        if (!result.Succeeded)
        //        {
        //            // Post-back for handling errors
        //            var fieldErrors = new List<string>();

        //            TempData["error_fields"] = fieldErrors;
        //            TempData["error_message"] = "Invalid username or password";
        //            TempData["postback_username"] = username;

        //            return Redirect("/setup");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogException(ex);
        //    }
        //    return Redirect("/");
        //}
    }
}
