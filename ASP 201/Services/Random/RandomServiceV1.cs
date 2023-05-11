using ASP_201.Models.User;
using ASP_201.Services.Hash;

namespace ASP_201.Services.Random
{
    public class RandomServiceV1 : IRandomService
    {
        private readonly String symbolsDataSet = "abcdefghijklmnopqrstuvwxyz0123456789";
        private readonly String _safeChars= new String(
            Enumerable.Range(20,107).Select(x=>(char)x).ToArray());
        private readonly System.Random random = new System.Random();
        private readonly IHashService _hashService;

        public RandomServiceV1(IHashService hashService)
        {
            _hashService = hashService;
        }

        public string GeneratePhotoName(string photoName,string path)
        {
            // Генеруємо для файла нове ім'я, але зберігаємо розширення
            String savedName = "";
            String resultPath = "";
            String ext = Path.GetExtension(photoName);
            // TODO: перевірити розширення на перелік дозволених
            savedName = _hashService.Hash(
            photoName + DateTime.Now)[..16]
                + ext;
            resultPath = path + savedName;

            while (System.IO.File.Exists(resultPath))
            {
                savedName = _hashService.Hash(
                    photoName + DateTime.Now)[..16]
                    + ext;
                resultPath = path + savedName;
            }
            return savedName;
        }

        public string AvatarPhotoName(string photoName)
        {
            // Генеруємо для файла нове ім'я, але зберігаємо розширення
            String savedName = "";
            String ext = Path.GetExtension(photoName);
            // TODO: перевірити розширення на перелік дозволених
            savedName = _hashService.Hash(
            photoName + DateTime.Now)[..16]
                + ext;
            String path = "wwwroot/avatars/" + savedName;

            while (System.IO.File.Exists(path))
            {
                savedName = _hashService.Hash(
                    photoName + DateTime.Now)[..16]
                    + ext;
                path = "wwwroot/avatars/" + savedName;
            }
            return savedName;
        }

        public String ConfirmCode(int length)
        {
            string result = "";
            
            for(int i=0;i<length;i++)
            { 
                int index=random.Next(symbolsDataSet.Length);
                result += symbolsDataSet[index];
            }
            return result;
        }

        public string RandomString(int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = _safeChars[random.Next(_safeChars.Length)];
            return new string(chars);
        }
    }
}
