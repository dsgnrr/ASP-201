namespace ASP_201.Services.Transliterate
{
    public interface ITransliterationService
    {
        public string Transliterate(string source);
        public string TransliterateV2(string source);
    }
}
