using BepInEx.Configuration;
using BepInEx.Logging;
using MyEmotions.Emotions.Situations;
using UnityEngine;

namespace MyEmotions.Emotions.Configuration;

/// <summary>
/// Exposes MyEmotions settings through BepInEx Configuration Manager.
/// </summary>
public static class MyEmotionsConfigEntries
{
    private const float MaximumCooldownSeconds = 3600f;

    /// <summary>
    /// Creates the editable BepInEx entries and connects them to the loaded options.
    /// </summary>
    public static MyEmotionsOptions Bind(
        ConfigFile config,
        MyEmotionsOptions options,
        ManualLogSource logger,
        string language = "RU")
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        // Keep the existing Russian keys so installed configurations retain their overrides.
        var prefix = language == "RU" ? string.Empty : $"{language}.";
        var enabled = config.Bind(
            $"{prefix}General",
            "Enabled",
            options.Enabled,
            "Enables or disables all emotion phrases.");
        enabled.SettingChanged += (_, _) => options.Enabled = enabled.Value;
        options.Enabled = enabled.Value;

        var globalCooldown = config.Bind(
            $"{prefix}General",
            "GlobalPhraseCooldownSeconds",
            (float)Math.Max(0, options.GlobalPhraseCooldownSeconds),
            new ConfigDescription(
                "Minimum delay between any two emotion phrases, in seconds.",
                new AcceptableValueRange<float>(0, MaximumCooldownSeconds)));
        globalCooldown.SettingChanged += (_, _) =>
            options.GlobalPhraseCooldownSeconds = Math.Max(0, globalCooldown.Value);
        options.GlobalPhraseCooldownSeconds = Math.Max(0, globalCooldown.Value);

        foreach (var situation in Enum.GetValues(typeof(EmotionSituation))
                     .Cast<EmotionSituation>())
        {
            var defaults = options.Phrases.TryGetValue(situation, out var option)
                ? option
                : new PhraseOption();
            var section = $"{prefix}Situations.{situation}";

            var situationEnabled = config.Bind(
                section,
                "Enabled",
                defaults.Enabled,
                $"Enables the {situation} trigger.");
            situationEnabled.SettingChanged += (_, _) =>
                defaults.Enabled = situationEnabled.Value;
            defaults.Enabled = situationEnabled.Value;

            var cooldown = config.Bind(
                section,
                "CooldownSeconds",
                (float)Math.Max(0, defaults.CooldownSeconds),
                new ConfigDescription(
                    "Minimum delay for this trigger, in seconds.",
                    new AcceptableValueRange<float>(0, MaximumCooldownSeconds)));
            cooldown.SettingChanged += (_, _) =>
                defaults.CooldownSeconds = Math.Max(0, cooldown.Value);
            defaults.CooldownSeconds = Math.Max(0, cooldown.Value);

            var phrases = config.Bind(
                section,
                "Phrases",
                SerializePhrases(defaults.Phrases),
                new ConfigDescription(
                    $"{language} phrase templates. "
                    + "One phrase per line. Empty lines are ignored.",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = DrawPhraseList
                    }));
            phrases.SettingChanged += (_, _) =>
                defaults.Phrases = ParsePhrases(phrases.Value);
            defaults.Phrases = ParsePhrases(phrases.Value);

            options.Phrases[situation] = defaults;
        }

        LegacyConfigMigrator.Migrate(config, options, logger, language);

        logger.LogInfo(
            $"Registered {Enum.GetValues(typeof(EmotionSituation)).Length} {language} situations in BepInEx Configuration Manager.");
        return options;
    }

    private static string SerializePhrases(IEnumerable<string> phrases)
    {
        return string.Join("\\n", phrases);
    }

    private static List<string> ParsePhrases(string value)
    {
        return DecodePhraseText(value)
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Select(phrase => phrase.Trim())
            .Where(phrase => !string.IsNullOrWhiteSpace(phrase))
            .ToList();
    }

    private static string DecodePhraseText(string value)
    {
        return value.Replace("\\n", "\n");
    }

    private static void DrawPhraseList(ConfigEntryBase setting)
    {
        if (setting is not ConfigEntry<string> phraseEntry)
        {
            return;
        }

        var displayedValue = DecodePhraseText(phraseEntry.Value);
        var editedValue = GUILayout.TextArea(
            displayedValue,
            GUILayout.MinHeight(90f));
        var serializedValue = editedValue
            .Replace("\r\n", "\n")
            .Replace("\n", "\\n");

        if (!string.Equals(serializedValue, phraseEntry.Value, StringComparison.Ordinal))
        {
            phraseEntry.Value = serializedValue;
        }
    }
}
