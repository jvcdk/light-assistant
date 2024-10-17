namespace LightAssistant.Utils;

public static class StringExtensions
{
    public static string CamelCaseToSentence(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        return String.Concat(input.Select((currentChar, index) => {
            if(!Char.IsUpper(currentChar) || index == 0)
                return currentChar.ToString();

            return " " + Char.ToLower(currentChar);
        }));
    }
}
