namespace ASP_201.Services.Random
{
    public class RandomServiceV1 : IRandomService
    {
        private readonly String symbolsDataSet = "abcdefghijklmnopqrstuvwxyz0123456789";
        private readonly String _safeChars= new String(
            Enumerable.Range(20,107).Select(x=>(char)x).ToArray());
        private readonly System.Random random = new System.Random();
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
