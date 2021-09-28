using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.logic.interfaces;
using chapterone.web.viewmodels;
using chapterone.shared.utils;
using Microsoft.AspNetCore.Mvc;

namespace chapterone.web.controllers
{
    /// <summary>
    /// Watchlist API
    /// </summary>
    public class WatchlistController : Controller
    {
        private IDatabaseRepository<TwitterWatchlistProfile> _twitterWatchlist;

        /// <summary>
        /// Constructor
        /// </summary>
        public WatchlistController(IDatabaseRepository<TwitterWatchlistProfile> userRepo)
        {
            _twitterWatchlist = userRepo;
        }


        /// <summary>
        /// Get the watchlist for the current user
        /// </summary>
        [HttpGet("watchlist")]
        public async Task<IActionResult> GetWatchlist()
        {
            var profiles = await _twitterWatchlist.QueryAsync(x => true);

            var vm = new WatchlistViewModel()
            {
                Profiles = profiles.Select(x => new ProfileViewModel(x))
            };

            return View("~/views/Watchlist.cshtml", vm);
        }
    }
}