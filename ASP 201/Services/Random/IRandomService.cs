namespace ASP_201.Services.Random
{
    public interface IRandomService
    {
        String ConfirmCode(int length);
        String RandomString(int length);
        String AvatarPhotoName(string photoName);
        String GeneratePhotoName(string photoName,string path);
    }
}
