using System.ComponentModel.DataAnnotations;

namespace chapterone.shared.models
{
    public class UserModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
        //[Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "New password")]
        //public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm new password")]
        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        //public string ConfirmPassword { get; set; }
        [Required]
        [Display(Name = "Role")]
        public string RoleId { get; set; }
    }
}
