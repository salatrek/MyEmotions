using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx.Logging;
using MyEmotions.Emotions.Situations;

namespace MyEmotions.Emotions.Configuration;

/// <summary>
/// Loads MyEmotions settings from emotionsConfigs.json.
/// </summary>
public static class MyEmotionsOptionsFile
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Loads and validates settings from the specified JSON file.</summary>
    public static MyEmotionsOptions Load(string path, ManualLogSource logger)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A configuration path is required.", nameof(path));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        var json = File.ReadAllText(path);
        var options = JsonSerializer.Deserialize<MyEmotionsOptions>(json, SerializerOptions);

        if (options is null)
        {
            throw new InvalidDataException($"Configuration file '{path}' is empty.");
        }

        if (options.GlobalPhraseCooldownSeconds < 0)
        {
            logger.LogWarning(
                $"Negative GlobalPhraseCooldownSeconds in '{Path.GetFileName(path)}' was replaced with 0.");
            options.GlobalPhraseCooldownSeconds = 0;
        }

        options.Phrases ??= new Dictionary<EmotionSituation, PhraseOption>();
        options.Phrases = options.Phrases.ToDictionary(
            pair => pair.Key,
            pair => NormalizePhraseOption(pair.Key, pair.Value, logger, path));

        var hasAnyUsablePhrase = options.Phrases.Values
            .SelectMany(option => option.Phrases)
            .Any();

        if (!hasAnyUsablePhrase)
        {
            logger.LogWarning(
                $"No non-empty phrase options configured for any situation in '{Path.GetFileName(path)}'. "
                + "EmotionPhraseProvider cannot provide phrases.");
        }

        return options;
    }

    private static PhraseOption NormalizePhraseOption(
        EmotionSituation situation,
        PhraseOption? option,
        ManualLogSource logger,
        string path)
    {
        option ??= new PhraseOption();

        if (option.CooldownSeconds < 0)
        {
            logger.LogWarning(
                $"Negative cooldown for situation '{situation}' in '{Path.GetFileName(path)}' was replaced with 0.");
            option.CooldownSeconds = 0;
        }

        option.Phrases = option.Phrases?
            .Where(phrase => !string.IsNullOrWhiteSpace(phrase))
            .Select(phrase => phrase.Trim())
            .ToList()
            ?? new List<string>();

        return option;
    }
}
