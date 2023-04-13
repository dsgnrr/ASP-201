using ASP_201.Services.Hash;

namespace ASP_201.Services.Kdf
{
    public class HashKdfService : IKdfService
    {
        private readonly IHashService _hashService;

        public HashKdfService(IHashService hashService)
        {
            _hashService = hashService;
        }

        public String GetDerivedKey(string password, string salt)
        {
            return _hashService.Hash(salt + password);
        }
    }
}
