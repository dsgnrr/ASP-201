namespace ASP_201.Services.Validation
{
    public interface IValidationService
    {
        bool Validate(String source,params ValidationTerms[] terms);
    }
}
