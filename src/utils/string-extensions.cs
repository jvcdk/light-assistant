namespace LightAssistant.Utils;

public static class StringExtensions
{
    public static string CamelCaseToSentence(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        return string.Concat(input.Select((currentChar, index) => {
            if(!char.IsUpper(currentChar) || index == 0)
                return currentChar.ToString();

            return " " + char.ToLower(currentChar);
        }));
    }

    public static string SentenceToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return string.Concat(input.Split(' ').Select((word, index) => {
            if (word.Length == 0)
                return string.Empty;
            return char.ToUpper(word[0]) + word[1..].ToLower();
        }));
    }
}
