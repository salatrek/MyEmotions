namespace MyEmotions.Emotions.Configuration;

/// <summary>
/// Optional metadata understood by BepInEx Configuration Manager.
/// The manager safely ignores this type when it is not installed.
/// </summary>
internal sealed class ConfigurationManagerAttributes
{
    public System.Action<BepInEx.Configuration.ConfigEntryBase>? CustomDrawer;
}
