namespace MyEmotions.Emotions.Situations;

/// <summary>
/// Fixed situations supported by the mod.
/// New situations are added here and documented in README.md.
/// </summary>
public enum EmotionSituation
{
    /// <summary>Rain has started in the current world.</summary>
    RainStarted,

    /// <summary>The local player has become wet.</summary>
    WetStarted,

    /// <summary>The local player has become cold.</summary>
    ColdStarted,

    /// <summary>Fog has appeared around the player.</summary>
    FogStarted,

    /// <summary>The player has started sailing on a ship.</summary>
    SailingStarted,

    /// <summary>The player has stepped onto a ship.</summary>
    BoardedShip,

    /// <summary>The player has equipped a bow.</summary>
    BowEquipped,

    /// <summary>The player's arrows have run out while using a bow.</summary>
    ArrowsDepleted,

    /// <summary>The player has equipped a non-bow weapon.</summary>
    WeaponDrawn,

    /// <summary>The player has opened or activated a cauldron.</summary>
    CauldronOpened,

    /// <summary>The player has started crafting at a cauldron.</summary>
    CauldronCookingStarted,

    /// <summary>Crafting at a cauldron has finished.</summary>
    CauldronCookingFinished,

    /// <summary>The player has successfully eaten food.</summary>
    FoodEaten,

    /// <summary>A hostile creature has attacked the player.</summary>
    HostileCreatureAttack,

    /// <summary>The player has used a portal.</summary>
    PortalUsed,

    /// <summary>The player has entered the swamp.</summary>
    SwampEntered,

    /// <summary>The player has entered the plains.</summary>
    PlainsEntered,

    /// <summary>The player has entered the mountains.</summary>
    MountainsEntered,

    /// <summary>The player has entered the Mistlands.</summary>
    MistlandsEntered,

    /// <summary>A deathsquito has attacked the player.</summary>
    DeathsquitoAttack,

    /// <summary>A raid has started.</summary>
    RaidStarted,

    /// <summary>A boss has been summoned.</summary>
    BossSummoned,

    /// <summary>The local player has died.</summary>
    PlayerDeath,

    /// <summary>The local player has respawned.</summary>
    PlayerRespawned,

    /// <summary>The player has become encumbered.</summary>
    EncumberedStarted,

    /// <summary>The player's health has dropped to a dangerous level.</summary>
    LowHealthStarted,

    /// <summary>The equipped weapon has broken.</summary>
    WeaponBroken,

    /// <summary>The player has taken fall damage.</summary>
    FallDamageTaken,

    /// <summary>The player has caught a fish.</summary>
    FishingCatch,

    /// <summary>A tree has been felled.</summary>
    TreeFelled,

    /// <summary>The player has tamed a creature.</summary>
    TamedCreature,

    /// <summary>The rested effect has started.</summary>
    RestedBuffStarted,

    /// <summary>The rested effect has expired.</summary>
    RestedBuffExpired,

    /// <summary>The player has completed crafting an item.</summary>
    CraftingCompleted,

    /// <summary>The player has entered shelter.</summary>
    ShelterEntered,

    /// <summary>The player has harvested a planted crop.</summary>
    PlantHarvested,

    /// <summary>The player has gathered a wild plant.</summary>
    WildPlantGathered
}
