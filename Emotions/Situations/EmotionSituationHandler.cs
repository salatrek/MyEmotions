using MyEmotions.Emotions.Chat;
using MyEmotions.Emotions.Phrases;

namespace MyEmotions.Emotions.Situations;

/// <summary>
/// Coordinates a detected situation, phrase selection and emotion output.
/// </summary>
public sealed class EmotionSituationHandler
{
    private readonly EmotionPhraseProvider _phraseProvider;
    private readonly IChatService _chatService;

    /// <summary>Creates a handler with phrase and chat dependencies.</summary>
    public EmotionSituationHandler(
        EmotionPhraseProvider phraseProvider,
        IChatService chatService)
    {
        _phraseProvider = phraseProvider
            ?? throw new ArgumentNullException(nameof(phraseProvider));
        _chatService = chatService
            ?? throw new ArgumentNullException(nameof(chatService));
    }

    /// <summary>
    /// Selects a phrase for the detected situation and displays it above the player.
    /// Returns <see langword="false"/> when no phrase is configured.
    /// </summary>
    public bool TryHandle(
        EmotionSituation situation,
        IReadOnlyDictionary<string, string>? context = null)
    {
        var phrase = _phraseProvider.GetPhrase(situation, context);
        if (phrase is null)
        {
            return false;
        }

        _chatService.Send(phrase);
        return true;
    }
}
