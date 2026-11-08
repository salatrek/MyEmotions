using BepInEx;
using HarmonyLib;
using System.Reflection;
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
    private const string PluginVersion = "0.1.0";
    private const string ConfigurationFileName = "emotionsConfigs.json";

    private readonly Harmony _harmony = new(PluginGuid);
    private EmotionPhraseProvider? _phraseProvider;
    private EmotionSituationHandler? _situationHandler;

    private void Awake()
    {
        var configurationPath = Path.Combine(Paths.ConfigPath, ConfigurationFileName);

        if (!EnsureConfigurationFile(configurationPath))
        {
            return;
        }

        var options = MyEmotionsOptionsFile.Load(configurationPath, Logger);
        options = MyEmotionsConfigEntries.Bind(Config, options, Logger);
        _phraseProvider = new EmotionPhraseProvider(options);
        _situationHandler = new EmotionSituationHandler(
            _phraseProvider,
            new ValheimChatService(Logger));

        SituationDetectionPatches.Initialize(
            _situationHandler,
            Logger);
        SituationDetectionPatches.ApplyPatches(_harmony);

        Logger.LogInfo("MyEmotions initialized.");
    }

    private bool EnsureConfigurationFile(string configurationPath)
    {
        if (File.Exists(configurationPath))
        {
            return true;
        }

        try
        {
            using var template = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("MyEmotions.emotionsConfigs.json");

            if (template is null)
            {
                Logger.LogError(
                    "Embedded emotionsConfigs.json was not found; MyEmotions will not provide phrases.");
                return false;
            }

            Directory.CreateDirectory(Paths.ConfigPath);
            using var configuration = File.Create(configurationPath);
            template.CopyTo(configuration);

            Logger.LogInfo($"Created default configuration at '{configurationPath}'.");
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError(
                $"Could not create configuration file '{configurationPath}': {exception}");
            return false;
        }
    }

    private void OnDestroy()
    {
        SituationDetectionPatches.Shutdown();
        _harmony.UnpatchSelf();
    }
}
