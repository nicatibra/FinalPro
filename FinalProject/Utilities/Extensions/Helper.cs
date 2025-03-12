namespace FinalProject.Utilities.Extensions
{
    public class Helper
    {
        public static string InsertLineBreaks(string text, int wordsPerLine = 3)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var words = text.Split(' ');
            for (int i = wordsPerLine; i < words.Length; i += wordsPerLine + 1)
            {
                words[i] = "<br>" + words[i];
            }

            return string.Join(" ", words);
        }
    }
}
