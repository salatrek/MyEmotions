namespace MyEmotions.Emotions.Phrases;

/// <summary>Maps the language selected in Valheim to a supported phrase language.</summary>
public static class EmotionLanguage
{
    /// <summary>Reads the current game language when a phrase is requested.</summary>
    public static string? GetSelectedGameLanguage()
    {
        return Localization.instance?.GetSelectedLanguage();
    }

    /// <summary>All languages other than Russian use English phrases.</summary>
    public static bool IsRussian(string? language)
    {
        return string.Equals(language, "Russian", StringComparison.OrdinalIgnoreCase);
    }
}
