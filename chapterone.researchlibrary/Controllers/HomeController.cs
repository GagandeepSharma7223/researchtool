using chapterone.web.viewmodels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace chapterone.web.controllers
{
    public class HomeController : Controller
    {
        [HttpGet("error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
