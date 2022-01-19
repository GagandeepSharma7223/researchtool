using System.ComponentModel.DataAnnotations;

namespace chapterone.web.Areas.Admin.ViewModels
{
    public class RoleViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
