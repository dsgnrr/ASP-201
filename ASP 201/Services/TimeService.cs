namespace ASP_201.Services
{
    public class TimeService
    {
        public DateTime GetMoment()
        {
            return DateTime.Now.ToLocalTime();
        }
    }
}
