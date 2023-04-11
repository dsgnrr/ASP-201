namespace ASP_201.Models.User
{
    public class RegisterValidationModel
    {
        public String LoginMessage { get; set; } = null;
        public String PasswordMessage { get; set; } = null;
        public String RepeatPasswordMessage { get; set; } = null;
        public String EmailMessage { get; set; } = null;
        public String RealNameMessage { get; set; } = null;
        public String IsAgreeMessage { get; set; } = null;
        public String AvatarMessage { get; set; } = null;
    }
}
