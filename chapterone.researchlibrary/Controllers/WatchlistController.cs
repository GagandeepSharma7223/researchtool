using chapterone.data.interfaces;
using chapterone.web.viewmodels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace chapterone.web.controllers
{
    /// <summary>
    /// Watchlist API
    /// </summary>
    /// 
    [Authorize]
    public class WatchlistController : Controller
    {
        private ITwitterWatchlistRepository _twitterWatchlist;

        /// <summary>
        /// Constructor
        /// </summary>
        public WatchlistController(ITwitterWatchlistRepository twitterWatchlistRepository)
        {
            _twitterWatchlist = twitterWatchlistRepository;
        }


        /// <summary>
        /// Get the watchlist for the current user
        /// </summary>
        [HttpGet("watchlist")]
        public async Task<IActionResult> GetWatchlist()
        {
            var profiles = await _twitterWatchlist.QueryAsync(x => true, pageNumber: 1, pageSize: 20);

            var vm = new WatchlistViewModel()
            {
                Profiles = profiles.Select(x => new ProfileViewModel(x))
            };

            return View("~/views/Watchlist.cshtml", vm);
        }
    }
}