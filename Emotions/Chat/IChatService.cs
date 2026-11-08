namespace MyEmotions.Emotions.Chat;

/// <summary>
/// Displays an emotion phrase for the local player.
/// </summary>
public interface IChatService
{
    /// <summary>Displays the specified phrase above the local player.</summary>
    void Send(string message);
}
