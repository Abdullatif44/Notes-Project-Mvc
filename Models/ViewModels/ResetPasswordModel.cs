namespace Notes.Models.ViewModels
{
    public class ResetPasswordModel
    {
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string ReturnToken { get; set; }
        public string Email { get; set; }


    }
}
