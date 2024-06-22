using System.Reflection;

namespace LightAssistant.Utils;

public static class EnumerableExt
{
    public static IEnumerable<T> Wrap<T>(T element)
    {
        yield return element;
    }

    public static IEnumerable<T> EnumeratePropertiesOfType<T>(this object self) => self.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(p => p.GetValue(self))
            .OfType<T>();
}
