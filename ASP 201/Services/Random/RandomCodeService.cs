namespace ASP_201.Services.Random
{
    public class RandomCodeService : IRandomService
    {
        private string symbolsDataSet = "abcdefghijklmnopqrstuvwxyz0123456789";
        public string GetRandomCode(int length)
        {
            string result = "";
            System.Random random = new System.Random(); 
            for(int i=0;i<length;i++)
            { 
                int index=random.Next(symbolsDataSet.Length);
                result += symbolsDataSet[index];
            }
            return result;
        }
    }
}
