namespace ASP_201.Services.Kdf
{
    /// <summary>
    ///  Key Derivation Function Service (RFC 8018)
    /// </summary>
    public interface IKdfService
    {
        /// <summary>
        /// Mixing password and salt to make a derived key
        /// </summary>
        /// <param name="password">Password</param>
        /// <param name="salt">Salt</param>
        /// <returns>Derivaed Key as string</returns>
        String GetDerivedKey(String password, String salt);
    }
}
