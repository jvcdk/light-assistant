namespace LightAssistant.Utils;

public static class DictionaryExtension
{
    public static bool CompareEqual<TKey, TValue>(this Dictionary<TKey, TValue> self, Dictionary<TKey, TValue> other) where TKey : notnull
    {
        if (self.Count != other.Count)
            return false;

        foreach (var (key, value) in self) {
            if (!other.TryGetValue(key, out var otherValue))
                return false;

            if(value == null) {
                if(otherValue != null)
                    return false;
                continue;
            }

            if (!value.Equals(otherValue))
                return false;
        }

        return true;
    }
}
