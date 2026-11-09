using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using BepInEx.Logging;
using MyEmotions.Emotions.Situations;

namespace MyEmotions.Emotions.Configuration;

/// <summary>
/// Loads MyEmotions settings from a language-specific JSON file.
/// </summary>
public static class MyEmotionsOptionsFile
{
    /// <summary>Russian phrase configuration filename.</summary>
    public const string RussianFileName = "emotionsConfigs.RU.json";

    /// <summary>English phrase configuration filename.</summary>
    public const string EnglishFileName = "emotionsConfigs.EN.json";

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

    /// <summary>Creates missing language files and migrates the old combined file without modifying it.</summary>
    public static void EnsureLocalizedFiles(string directory, ManualLogSource logger)
    {
        Directory.CreateDirectory(directory);
        foreach (var fileName in new[] { RussianFileName, EnglishFileName })
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
            {
                continue;
            }

            using var template = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"MyEmotions.{fileName}")
                ?? throw new InvalidDataException($"Embedded '{fileName}' was not found.");
            var options = JsonSerializer.Deserialize<MyEmotionsOptions>(template, SerializerOptions)
                ?? throw new InvalidDataException($"Embedded '{fileName}' is empty.");
            var legacyPath = Path.Combine(directory, "emotionsConfigs.json");
            if (File.Exists(legacyPath))
            {
                var legacy = Load(legacyPath, logger);
                if (fileName == RussianFileName)
                {
                    options = legacy;
                }
                else
                {
                    options.Enabled = legacy.Enabled;
                    options.GlobalPhraseCooldownSeconds = legacy.GlobalPhraseCooldownSeconds;
                    // EnglishPhrases existed in the earlier combined format only.
                    using var document = JsonDocument.Parse(File.ReadAllText(legacyPath),
                        new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
                    var legacyPhrases = FindProperty(document.RootElement, "Phrases");
                    foreach (var pair in legacy.Phrases)
                    {
                        if (!options.Phrases.TryGetValue(pair.Key, out var translated))
                        {
                            continue;
                        }

                        translated.Enabled = pair.Value.Enabled;
                        translated.CooldownSeconds = pair.Value.CooldownSeconds;
                        var entry = FindProperty(legacyPhrases, pair.Key.ToString());
                        var phrases = FindProperty(entry, "EnglishPhrases");
                        if (phrases.ValueKind == JsonValueKind.Array)
                        {
                            translated.Phrases = JsonSerializer.Deserialize<List<string>>(
                                phrases.GetRawText(), SerializerOptions) ?? new List<string>();
                        }
                    }
                }
            }

            var writeOptions = new JsonSerializerOptions(SerializerOptions)
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            File.WriteAllText(path, JsonSerializer.Serialize(options, writeOptions));
            logger.LogInfo($"Created configuration at '{path}'.");
        }
    }

    private static JsonElement FindProperty(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object
            ? element.EnumerateObject()
                .FirstOrDefault(property => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)).Value
            : default;
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
