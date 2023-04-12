namespace ASP_201.Services.Hash
{
    public interface IHashService
    {
        /// <summary>
        /// Обчислює хеш від рядкового аргументу та подає його у гексадецимальному вигляді
        /// </summary>
        /// <param name="text">Вхідний текст</param>
        /// <returns>Гексадецимальний рядок з хеш-образом тексту</returns>
        String Hash(String text);
    }
}
