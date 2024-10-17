using System.Reflection;

namespace LightAssistant.Utils;

public static class EnumerableExt
{
    public static IEnumerable<T> Wrap<T>(T element)
    {
        yield return element;
    }

    public static IEnumerable<T> EnumeratePropertiesOfType<T>(this object self) =>
        self.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(p => p.GetValue(self))
            .OfType<T>();

    public static IEnumerable<(PropertyInfo prop, T attr)> EnumeratePropertiesWithAttribute<T>(this Type self) where T: Attribute =>
        self
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(prop => (prop, attr: prop.GetCustomAttribute<T>()))
            .Where(tuple => tuple.attr != null)!;

    public static IEnumerable<(PropertyInfo prop, T attr)> EnumeratePropertiesWithAttribute<T>(this object self) where T: Attribute =>
        self.GetType().EnumeratePropertiesWithAttribute<T>();

    public static IEnumerable<(MethodInfo method, T attr)> EnumerateMethodsWithAttribute<T>(this object self) where T: Attribute =>
        self.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(prop => (prop, attr: prop.GetCustomAttribute<T>()))
            .Where(tuple => tuple.attr != null)!
            .ToList()!;

    public static IEnumerable<(MethodInfo method, ParameterInfo param)> EnumerateMethodsWithParam<T>(this object self) where T: class =>
        self.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(prop => (prop, param: prop.GetParameters()))
            .Where(tuple => tuple.param.Length == 1 && tuple.param[0].ParameterType == typeof(T))
            .Select(tuple => (tuple.prop, tuple.param[0]))
            .ToList()!;
}
