namespace MyEmotions.Emotions.Configuration;

/// <summary>
/// Configurable phrase collection and its situation-specific cooldown.
/// </summary>
public sealed class PhraseOption
{
    /// <summary>Enables or disables this situation.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Phrase templates in the language of the containing file.</summary>
    public List<string> Phrases { get; set; } = new();

    /// <summary>Minimum delay between phrases for this situation, in seconds.</summary>
    public double CooldownSeconds { get; set; }

}
