using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace MyEmotions.Emotions.Chat;

/// <summary>
/// Displays emotion phrases above the local player without adding them to chat history.
/// </summary>
public sealed class ValheimChatService : IChatService
{
    private readonly ManualLogSource _logger;

    /// <summary>Creates a chat service backed by BepInEx logging.</summary>
    public ValheimChatService(ManualLogSource logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void Send(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("A chat message is required.", nameof(message));
        }

        var chat = global::Chat.instance;
        var player = Player.m_localPlayer;
        if (chat is null || player is null)
        {
            _logger.LogWarning("Valheim world text is not available; the emotion phrase was not displayed.");
            return;
        }

        var addInworldText = AccessTools.Method(typeof(global::Chat), "AddInworldText");
        if (addInworldText is null)
        {
            _logger.LogError(
                "Valheim Chat.AddInworldText was not found; the emotion phrase was not displayed "
                + "to avoid adding it to the chat window.");
            return;
        }

        try
        {
            var parameters = addInworldText.GetParameters();
            if (parameters.Length != 6)
            {
                _logger.LogError(
                    $"Valheim Chat.AddInworldText has an unexpected signature with "
                    + $"{parameters.Length} parameters; the emotion phrase was not displayed.");
                return;
            }

            var userInfo = Activator.CreateInstance(parameters[4].ParameterType);
            var position = player.transform.position;
            var talkerId = player.GetZDOID().UserID;

            addInworldText.Invoke(
                chat,
                new object?[]
                {
                    player.gameObject,
                    talkerId,
                    position,
                    global::Talker.Type.Normal,
                    userInfo,
                    message
                });

            _logger.LogInfo($"Displayed emotion phrase above the player: '{message}'.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                $"Could not display emotion phrase above the player: {exception}");
        }
    }
}
