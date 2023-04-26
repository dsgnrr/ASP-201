namespace ASP_201.Services.Email
{
    public interface IEmailService
    {
        bool Send(string mailTemplate,object model);
    }
}
