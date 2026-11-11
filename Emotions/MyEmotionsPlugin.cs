using BepInEx;
using HarmonyLib;
using MyEmotions.Emotions.Chat;
using MyEmotions.Emotions.Configuration;
using MyEmotions.Emotions.Phrases;
using MyEmotions.Emotions.Situations;
using MyEmotions.Emotions.Detection;

namespace MyEmotions.Emotions;

/// <summary>
/// BepInEx entrypoint for MyEmotions.
/// </summary>
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class MyEmotionsPlugin : BaseUnityPlugin
{
    private const string PluginGuid = "salatrek.MyEmotions";
    private const string PluginName = "MyEmotions";
    private const string PluginVersion = "0.1.5";

    private readonly Harmony _harmony = new(PluginGuid);
    private EmotionPhraseProvider? _phraseProvider;
    private EmotionSituationHandler? _situationHandler;

    private void Awake()
    {
        try
        {
            MyEmotionsOptionsFile.EnsureLocalizedFiles(Paths.ConfigPath, Logger);
        }
        catch (Exception exception)
        {
            Logger.LogError($"Could not create language configurations: {exception}");
            return;
        }

        var russianOptions = MyEmotionsOptionsFile.Load(
            Path.Combine(Paths.ConfigPath, MyEmotionsOptionsFile.RussianFileName), Logger);
        var englishOptions = MyEmotionsOptionsFile.Load(
            Path.Combine(Paths.ConfigPath, MyEmotionsOptionsFile.EnglishFileName), Logger);
        MyEmotionsConfigEntries.Bind(Config, russianOptions, Logger, "RU");
        MyEmotionsConfigEntries.Bind(Config, englishOptions, Logger, "EN");
        _phraseProvider = new EmotionPhraseProvider(russianOptions, englishOptions);
        _situationHandler = new EmotionSituationHandler(
            _phraseProvider,
            new ValheimChatService(Logger));

        SituationDetectionPatches.Initialize(
            _situationHandler,
            Logger);
        SituationDetectionPatches.ApplyPatches(_harmony);

        Logger.LogInfo("MyEmotions initialized.");
    }

    private void OnDestroy()
    {
        SituationDetectionPatches.Shutdown();
        _harmony.UnpatchSelf();
    }
}
