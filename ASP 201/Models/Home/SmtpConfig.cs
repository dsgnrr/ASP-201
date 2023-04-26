namespace ASP_201.Models.Home
{
    public class SmtpConfig
    {
        public String Host      { get; set; } = null!;
        public String Email     { get; set; } = null!;
        public int Port         { get; set; } = 0;
        public bool Ssl         { get; set; } = false;

    }
}
