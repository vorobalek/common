using System.Collections.Generic;

namespace Common.Infrastructure.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<string, string?> ResetValues(this Dictionary<string, string?> dictionary,
        params string[] keys)
    {
        foreach (var key in keys) dictionary.ReplaceValue(key, null);
        return dictionary;
    }

    public static Dictionary<string, string?> ReplaceValues(this Dictionary<string, string?> dictionary,
        Dictionary<string, string?> replaceMap)
    {
        foreach (var (key, value) in replaceMap) dictionary.ReplaceValue(key, value);
        return dictionary;
    }

    public static Dictionary<string, string?> ReplaceValue(this Dictionary<string, string?> dictionary, string key,
        string? value)
    {
        if (!dictionary.ContainsKey(key))
            dictionary.Add(key, value);
        else
            dictionary[key] = value;
        return dictionary;
    }
}