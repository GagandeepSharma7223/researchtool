using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chapterone.web.viewmodels
{
    public class SettingsViewModel
    {
        /// <summary>
        /// List of users that can use this platform
        /// </summary>
        public IList<string> Users { get; set; } = new List<string>();


        /// <summary>
        /// List of twitter handles being watched
        /// </summary>
        public IList<string> Watchlist { get; set; } = new List<string>();
    }
}
