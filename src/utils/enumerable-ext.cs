namespace LightAssistant.Utils;

public static class EnumerableExt
{
    public static IEnumerable<T> Wrap<T>(T element)
    {
        yield return element;
    }
}
