namespace chapterone.shared.models
{
    public class PasswordChangeEmailVM
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordChangeTime { get; set; }
        public string PasswordResetUrl { get; set; }
        public bool IsForNewPasword { get; set; }
        public string UnsubscribeUrl { get; set; }
    }
}
