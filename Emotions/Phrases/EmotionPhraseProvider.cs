using MyEmotions.Emotions.Configuration;
using MyEmotions.Emotions.Situations;

namespace MyEmotions.Emotions.Phrases;

/// <summary>
/// Selects and renders a phrase for a situation.
/// </summary>
public sealed class EmotionPhraseProvider
{
    private readonly MyEmotionsOptions _options;
    private readonly Random _random;
    private readonly Dictionary<EmotionSituation, string> _previousPhrases = new();
    private readonly Dictionary<string, DateTimeOffset> _nextPhraseAvailableAt =
        new(StringComparer.OrdinalIgnoreCase);
    private DateTimeOffset _nextGlobalPhraseAvailableAt = DateTimeOffset.MinValue;

    /// <summary>Creates a provider backed by application options.</summary>
    public EmotionPhraseProvider(
        MyEmotionsOptions options,
        Random? random = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _random = random ?? new Random();
    }

    /// <summary>
    /// Returns a random configured phrase, or <see langword="null"/> when no phrase is configured.
    /// </summary>
    public string? GetPhrase(
        EmotionSituation situation,
        IReadOnlyDictionary<string, string>? context = null)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        if (now < _nextGlobalPhraseAvailableAt)
        {
            return null;
        }

        if (!TryGetUsablePhrases(situation, out var option, out var phrases))
        {
            return null;
        }

        var cooldownKey = GetCooldownKey(situation);
        if (_nextPhraseAvailableAt.TryGetValue(cooldownKey, out var nextSituationPhraseAvailableAt)
            && now < nextSituationPhraseAvailableAt)
        {
            return null;
        }

        var candidates = phrases;
        if (phrases.Length > 1
            && _previousPhrases.TryGetValue(situation, out var previousPhrase))
        {
            candidates = phrases
                .Where(phrase => !string.Equals(phrase, previousPhrase, StringComparison.Ordinal))
                .ToArray();

            if (candidates.Length == 0)
            {
                candidates = phrases;
            }
        }

        var phrase = candidates[_random.Next(candidates.Length)];
        _previousPhrases[situation] = phrase;
        _nextGlobalPhraseAvailableAt = now.AddSeconds(_options.GlobalPhraseCooldownSeconds);
        _nextPhraseAvailableAt[cooldownKey] = now.AddSeconds(option.CooldownSeconds);

        return RenderTemplate(phrase, context);
    }

    private static string GetCooldownKey(EmotionSituation situation)
    {
        return situation is EmotionSituation.CauldronOpened
            or EmotionSituation.CauldronCookingStarted
            or EmotionSituation.CauldronCookingFinished
            ? "CauldronCooking"
            : situation.ToString();
    }

    private bool TryGetUsablePhrases(
        EmotionSituation situation,
        out PhraseOption option,
        out string[] phrases)
    {
        if (_options.Phrases.TryGetValue(situation, out option!)
            && option is not null
            && option.Enabled)
        {
            phrases = option.Phrases
                .Where(phrase => !string.IsNullOrWhiteSpace(phrase))
                .Select(phrase => phrase.Trim())
                .ToArray();

            return phrases.Length > 0;
        }

        option = new PhraseOption();
        phrases = Array.Empty<string>();
        return false;
    }

    private static string RenderTemplate(
        string template,
        IReadOnlyDictionary<string, string>? context)
    {
        if (context is null || context.Count == 0)
        {
            return template;
        }

        foreach (var value in context)
        {
            template = template.Replace(
                $"{{{value.Key}}}",
                value.Value);
        }

        return template;
    }
}
