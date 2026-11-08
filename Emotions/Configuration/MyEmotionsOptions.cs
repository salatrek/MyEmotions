using MyEmotions.Emotions.Situations;

namespace MyEmotions.Emotions.Configuration;

/// <summary>
/// Editable application settings for MyEmotions.
/// </summary>
public sealed class MyEmotionsOptions
{
    /// <summary>Enables or disables the whole mod.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Minimum delay between any two phrases, in seconds.
    /// </summary>
    public double GlobalPhraseCooldownSeconds { get; set; } = 7;

    /// <summary>
    /// Maps a fixed situation to its phrases and situation-specific cooldown.
    /// </summary>
    public Dictionary<EmotionSituation, PhraseOption> Phrases { get; set; } = new();
}
