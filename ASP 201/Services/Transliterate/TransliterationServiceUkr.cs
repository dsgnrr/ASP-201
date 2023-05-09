using System.Text;

namespace ASP_201.Services.Transliterate
{
    public class TransliterationServiceUkr:ITransliterationService
    {
        private readonly string _ukrainianSimple = "АБВГҐДЕЗИІКЛМНОПРСТУФабвгґдезиклмнопрстуф";
        private readonly string _englishSimple = "ABVHGDEZYIKLMNOPRSTUFabvhgdezyklmnoprstuf";
        public string Transliterate(string source)
        {
            string result = source;
            result.Replace("зг", "zgh").Replace("Зг", "Zgh")
                .Replace("ь", "").Replace("Ь", "").Replace("\'", "");
            for (int i = 0; i < _ukrainianSimple.Length; i++)
            {
                result = result.Replace(_ukrainianSimple[i], _englishSimple[i]);
            }
            return result
                 .Replace("Ш", "Sh").Replace("ш", "sh")
                 .Replace("Х", "Kh").Replace("х", "kh")
                 .Replace("Ц", "Ts").Replace("ц", "ts")
                 .Replace("Ч", "Ch").Replace("ч", "Ch")
                 .Replace("Щ", "Shch").Replace("щ", "Shch")
                 .Replace(" Ї", " Yi").Replace(" ї", " yi")
                 .Replace("Ї", "I").Replace("ї", "i")
                 .Replace(" Й", " Y").Replace(" й", " i")
                 .Replace("Й", "Y").Replace("й", "i")
                 .Replace(" Є", " Ye").Replace(" є", " ye")
                 .Replace("Є", "Ie").Replace("є", "ie")
                 .Replace(" Ю", " Yu").Replace(" ю", " yu")
                 .Replace("Ю", "Iu").Replace("ю", "iu")
                 .Replace(" Я", " Ya").Replace(" я", " ya")
                 .Replace("Я", "Ia").Replace("я", "ia")
                 .Replace(' ', '-').Replace("+", "plus")
                 .Replace("?", "-");
        }
        private readonly Dictionary<char, string> _transliterationTable = new Dictionary<char, string>()
        {
            { 'а', "a" },
            { 'б', "b" },
            { 'в', "v" },
            { 'г', "h" },
            { 'ґ', "g" },
            { 'д', "d" },
            { 'е', "e" },
            { 'є', "ie" },
            { 'ж', "zh" },
            { 'з', "z" },
            { 'и', "y" },
            { 'і', "i" },
            { 'ї', "i" },
            { 'й', "i" },
            { 'к', "k" },
            { 'л', "l" },
            { 'м', "m" },
            { 'н', "n" },
            { 'о', "o" },
            { 'п', "p" },
            { 'р', "r" },
            { 'с', "s" },
            { 'т', "t" },
            { 'у', "u" },
            { 'ф', "f" },
            { 'х', "kh" },
            { 'ц', "ts" },
            { 'ч', "ch" },
            { 'ш', "sh" },
            { 'щ', "shch" },
            { 'ь', "" },
            { 'ю', "iu" },
            { 'я', "ia" },
            { 'А', "A" },
            { 'Б', "B" },
            { 'В', "V" },
            { 'Г', "H" },
            { 'Ґ', "G" },
            { 'Д', "D" },
            { 'Е', "E" },
            { 'Є', "Ye" },
            { 'Ж', "Zh" },
            { 'З', "Z" },
            { 'И', "Y" },
            { 'І', "I" },
            { 'Ї', "Yi" },
            { 'Й', "Y" },
            { 'К', "K" },
            { 'Л', "L" },
            { 'М', "M" },
            { 'Н', "N" },
            { 'О', "O" },
            { 'П', "P" },
            { 'Р', "R" },
            { 'С', "S" },
            { 'Т', "T" },
            { 'У', "U" },
            { 'Ф', "F" },
            { 'Х', "Kh" },
            { 'Ц', "Ts" },
            { 'Ч', "Ch" },
            { 'Ш', "Sh" },
            { 'Щ', "Shch" },
            { 'Ь', "" },
            { 'Ю', "Yu" },
            { 'Я', "Ya" }
        };
        public string TransliterateV2(string input)
        {
            StringBuilder output = new StringBuilder();
            foreach (char ch in input)
            {
                if (_transliterationTable.ContainsKey(ch))
                {
                    output.Append(_transliterationTable[ch]);
                }
                else
                {
                    output.Append(ch);
                }
            }



            return output.ToString();
        }
    }
}
