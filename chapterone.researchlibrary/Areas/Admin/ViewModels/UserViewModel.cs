using System.Collections.Generic;

namespace chapterone.web.Areas.Admin.ViewModels
{
    public class UserViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public string EmailConfirmed { get; set; }
        public int Version { get; set; }
        public string Id { get; set; }
    }
}
