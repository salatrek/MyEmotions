using HarmonyLib;
using BepInEx.Logging;
using MyEmotions.Emotions.Situations;
using UnityEngine;

namespace MyEmotions.Emotions.Detection;

/// <summary>
/// Harmony entrypoints that translate Valheim state changes into mod situations.
/// </summary>
internal static class SituationDetectionPatches
{
    private const float FogDensityThreshold = 0.01f;
    private const float FogStateStabilizationSeconds = 1f;

    private static EmotionSituationHandler? _situationHandler;
    private static ManualLogSource? _logger;
    private static bool _wasRaining;
    private static bool _wasFoggy;
    private static bool _hasFogCandidate;
    private static bool _fogCandidate;
    private static float _fogCandidateSince;
    private static bool _wasSailing;
    private static bool _wasOnShip;
    private static bool _wasBowEquipped;
    private static bool _wasWeaponDrawn;
    private static bool _hadArrowsWhileBow;
    private static bool _wasCauldronCrafting;
    private static bool _wasCrafting;
    private static bool _wasCraftingAtCauldron;
    private static bool _hasBiome;
    private static Heightmap.Biome _lastBiome;
    private static bool _wasEncumbered;
    private static bool _wasLowHealth;
    private static bool _wasWeaponBroken;
    private static bool _hasShelterState;
    private static bool _wasUnderRoof;
    private static bool _hasRestedState;
    private static bool _wasRested;

    public static void Initialize(
        EmotionSituationHandler situationHandler,
        ManualLogSource logger)
    {
        _situationHandler = situationHandler
            ?? throw new ArgumentNullException(nameof(situationHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _wasRaining = false;
        _wasFoggy = false;
        _hasFogCandidate = false;
        _fogCandidate = false;
        _fogCandidateSince = 0f;
        _wasSailing = false;
        _wasOnShip = false;
        _wasBowEquipped = false;
        _wasWeaponDrawn = false;
        _hadArrowsWhileBow = false;
        _wasCauldronCrafting = false;
        _wasCrafting = false;
        _wasCraftingAtCauldron = false;
        _hasBiome = false;
        _lastBiome = Heightmap.Biome.None;
        _wasEncumbered = false;
        _wasLowHealth = false;
        _wasWeaponBroken = false;
        _hasShelterState = false;
        _wasUnderRoof = false;
        _hasRestedState = false;
        _wasRested = false;
    }

    public static void Shutdown()
    {
        _situationHandler = null;
        _logger = null;
        _wasRaining = false;
        _wasFoggy = false;
        _hasFogCandidate = false;
        _fogCandidate = false;
        _fogCandidateSince = 0f;
        _wasSailing = false;
        _wasOnShip = false;
        _wasBowEquipped = false;
        _wasWeaponDrawn = false;
        _hadArrowsWhileBow = false;
        _wasCauldronCrafting = false;
        _wasCrafting = false;
        _wasCraftingAtCauldron = false;
        _hasBiome = false;
        _lastBiome = Heightmap.Biome.None;
        _wasEncumbered = false;
        _wasLowHealth = false;
        _wasWeaponBroken = false;
        _hasShelterState = false;
        _wasUnderRoof = false;
        _hasRestedState = false;
        _wasRested = false;
    }

    public static void ApplyPatches(Harmony harmony)
    {
        if (harmony is null)
        {
            throw new ArgumentNullException(nameof(harmony));
        }

        harmony.Patch(
            AccessTools.Method(typeof(EnvMan), "Update"),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(EnvManUpdatePostfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Player), "Update"),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(PlayerUpdatePostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Character),
                nameof(Character.Damage),
                new[] { typeof(HitData) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(CharacterDamagePostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Player),
                nameof(Player.TeleportTo),
                new[] { typeof(Vector3), typeof(Quaternion), typeof(bool) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(PlayerTeleportToPostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(CraftingStation),
                nameof(CraftingStation.Interact),
                new[] { typeof(Humanoid), typeof(bool), typeof(bool) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(CraftingStationInteractPostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Player),
                nameof(Player.SetCraftingStation),
                new[] { typeof(CraftingStation) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(PlayerSetCraftingStationPostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(InventoryGui),
                "DoCrafting",
                new[] { typeof(Player) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(InventoryGuiDoCraftingPostfix)));

        harmony.Patch(
            AccessTools.Method(typeof(InventoryGui), "OnCraftPressed"),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(InventoryGuiOnCraftPressedPostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(InventoryGui),
                "UpdateRecipe",
                new[] { typeof(Player), typeof(float) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(InventoryGuiUpdateRecipePostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Player),
                nameof(Player.EatFood),
                new[] { typeof(ItemDrop.ItemData) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(PlayerEatFoodPostfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Player), "OnDeath"),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(PlayerDeathPostfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Player), "OnRespawn"),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(PlayerRespawnPostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Fish),
                nameof(Fish.Pickup),
                new[] { typeof(Humanoid) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(FishPickupPostfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Tameable), "Tame"),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(TameableTamePostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(RandEventSystem),
                "SetActiveEvent",
                new[] { typeof(RandomEvent), typeof(bool) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(RandomEventSetActivePostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(TreeBase),
                nameof(TreeBase.Damage),
                new[] { typeof(HitData) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(TreeBaseDamagePostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Pickable),
                nameof(Pickable.UseItem),
                new[] { typeof(Humanoid), typeof(ItemDrop.ItemData) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(PickableUseItemPostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Pickable),
                nameof(Pickable.Interact),
                new[] { typeof(Humanoid), typeof(bool), typeof(bool) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(PickableInteractPostfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(ItemStand),
                nameof(ItemStand.UseItem),
                new[] { typeof(Humanoid), typeof(ItemDrop.ItemData) }),
            postfix: new HarmonyMethod(
                typeof(SituationDetectionPatches),
                nameof(ItemStandUseItemPostfix)));
    }

    private static void EnvManUpdatePostfix()
    {
        if (_situationHandler is null)
        {
            return;
        }

        var environment = EnvMan.instance?.GetCurrentEnvironment();
        var isRaining = environment is not null && environment.m_isWet;

        if (isRaining != _wasRaining)
        {
            _logger?.LogInfo($"Rain detector state changed: {isRaining}.");
        }

        if (isRaining)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.RainStarted);
            if (handled)
            {
                _logger?.LogInfo($"RainStarted detected. Phrase handled: {handled}.");
            }
        }

        _wasRaining = isRaining;
    }

    private static void PlayerUpdatePostfix(Player __instance)
    {
        if (_situationHandler is null || __instance != Player.m_localPlayer)
        {
            return;
        }

        var isSailing = __instance.GetControlledShip() is not null;
        var rawFoggy = RenderSettings.fog
            && RenderSettings.fogDensity >= FogDensityThreshold;
        var isFoggy = GetStableFogState(rawFoggy);
        var isOnShip = __instance.GetStandingOnShip() is not null;

        var rightItem = __instance.RightItem;
        var leftItem = __instance.LeftItem;
        var equippedItem = IsWeapon(rightItem) ? rightItem : leftItem;
        var isBowEquipped = equippedItem?.m_shared?.m_itemType
            == ItemDrop.ItemData.ItemType.Bow;
        var isWeaponDrawn = IsWeapon(equippedItem) && !isBowEquipped;
        var isEncumbered = __instance.IsEncumbered();
        var isLowHealth = __instance.GetHealthPercentage() <= 0.25f;
        var isUnderRoof = IsUnderRoof(__instance);
        var isRested = HasRestedEffect(__instance);
        var isWet = HasStatusEffect(__instance, "Wet");
        var isCold = HasStatusEffect(__instance, "Cold");
        var isWeaponBroken = equippedItem is not null
            && IsWeapon(equippedItem)
            && equippedItem.m_shared is not null
            && equippedItem.m_shared.m_useDurability
            && equippedItem.GetDurabilityPercentage() <= 0.01f;

        DetectBiomeTransition(__instance.GetCurrentBiome());

        if (isFoggy)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.FogStarted);
            if (handled)
            {
                _logger?.LogInfo($"FogStarted detected. Phrase handled: {handled}.");
            }
        }

        if (isSailing && !_wasSailing)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.SailingStarted);
            _logger?.LogInfo($"SailingStarted detected. Phrase handled: {handled}.");
        }

        if (isOnShip && !_wasOnShip && !isSailing)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.BoardedShip);
            _logger?.LogInfo($"BoardedShip detected. Phrase handled: {handled}.");
        }

        if (isBowEquipped && !_wasBowEquipped)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.BowEquipped);
            _logger?.LogInfo($"BowEquipped detected. Phrase handled: {handled}.");
        }

        if (isWeaponDrawn && !_wasWeaponDrawn)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.WeaponDrawn);
            _logger?.LogInfo($"WeaponDrawn detected. Phrase handled: {handled}.");
        }

        if (isEncumbered && !_wasEncumbered)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.EncumberedStarted);
            _logger?.LogInfo($"EncumberedStarted detected. Phrase handled: {handled}.");
        }

        if (isLowHealth && !_wasLowHealth)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.LowHealthStarted);
            _logger?.LogInfo($"LowHealthStarted detected. Phrase handled: {handled}.");
        }

        if (isWeaponBroken && !_wasWeaponBroken)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.WeaponBroken);
            _logger?.LogInfo($"WeaponBroken detected. Phrase handled: {handled}.");
        }

        if (isWet)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.WetStarted);
            if (handled)
            {
                _logger?.LogInfo($"WetStarted detected. Phrase handled: {handled}.");
            }
        }

        if (isCold)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.ColdStarted);
            if (handled)
            {
                _logger?.LogInfo($"ColdStarted detected. Phrase handled: {handled}.");
            }
        }

        if (_hasShelterState && isUnderRoof && !_wasUnderRoof)
        {
            var handled = _situationHandler.TryHandle(EmotionSituation.ShelterEntered);
            _logger?.LogInfo($"ShelterEntered detected. Phrase handled: {handled}.");
        }

        if (_hasRestedState && isRested != _wasRested)
        {
            var situation = isRested
                ? EmotionSituation.RestedBuffStarted
                : EmotionSituation.RestedBuffExpired;
            var handled = _situationHandler.TryHandle(situation);
            _logger?.LogInfo($"{situation} detected. Phrase handled: {handled}.");
        }

        if (isBowEquipped)
        {
            var ammo = __instance.GetAmmoItem();
            var hasArrows = ammo is not null && ammo.m_stack > 0;

            if (hasArrows)
            {
                _hadArrowsWhileBow = true;
            }
            else if (_hadArrowsWhileBow)
            {
                var handled = _situationHandler.TryHandle(EmotionSituation.ArrowsDepleted);
                _logger?.LogInfo($"ArrowsDepleted detected. Phrase handled: {handled}.");
                _hadArrowsWhileBow = false;
            }
        }
        else
        {
            _hadArrowsWhileBow = false;
        }

        _wasFoggy = isFoggy;
        _wasSailing = isSailing;
        _wasOnShip = isOnShip;
        _wasBowEquipped = isBowEquipped;
        _wasWeaponDrawn = isWeaponDrawn;
        _wasEncumbered = isEncumbered;
        _wasLowHealth = isLowHealth;
        _wasWeaponBroken = isWeaponBroken;
        _wasUnderRoof = isUnderRoof;
        _hasShelterState = true;
        _wasRested = isRested;
        _hasRestedState = true;
    }

    private static void DetectBiomeTransition(Heightmap.Biome currentBiome)
    {
        if (!_hasBiome)
        {
            _lastBiome = currentBiome;
            _hasBiome = true;
            return;
        }

        if (currentBiome == _lastBiome)
        {
            return;
        }

        _lastBiome = currentBiome;
        var situation = currentBiome switch
        {
            Heightmap.Biome.Swamp => EmotionSituation.SwampEntered,
            Heightmap.Biome.Plains => EmotionSituation.PlainsEntered,
            Heightmap.Biome.Mountain => EmotionSituation.MountainsEntered,
            Heightmap.Biome.Mistlands => EmotionSituation.MistlandsEntered,
            _ => (EmotionSituation?)null
        };

        if (situation is null || _situationHandler is null)
        {
            return;
        }

        var handled = _situationHandler.TryHandle(situation.Value);
        _logger?.LogInfo($"{situation.Value} detected. Phrase handled: {handled}.");
    }

    private static bool IsWeapon(ItemDrop.ItemData? item)
    {
        if (item?.m_shared is null)
        {
            return false;
        }

        return item.m_shared.m_itemType is
            ItemDrop.ItemData.ItemType.Bow
            or
            ItemDrop.ItemData.ItemType.OneHandedWeapon
            or ItemDrop.ItemData.ItemType.TwoHandedWeapon
            or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft
            or ItemDrop.ItemData.ItemType.Attach_Atgeir;
    }

    private static bool GetStableFogState(bool rawFoggy)
    {
        if (!_hasFogCandidate || rawFoggy != _fogCandidate)
        {
            _hasFogCandidate = true;
            _fogCandidate = rawFoggy;
            _fogCandidateSince = Time.unscaledTime;
        }

        if (rawFoggy == _wasFoggy)
        {
            return rawFoggy;
        }

        return Time.unscaledTime - _fogCandidateSince >= FogStateStabilizationSeconds
            ? _fogCandidate
            : _wasFoggy;
    }

    private static bool HasRestedEffect(Player player)
    {
        return HasStatusEffect(player, "Rested");
    }

    private static bool IsUnderRoof(Character character)
    {
        return character is Player player && HasStatusEffect(player, "Shelter");
    }

    private static bool HasStatusEffect(Player player, string effectName)
    {
        var statusEffects = player.GetSEMan()?.GetStatusEffects();
        return statusEffects is not null
            && statusEffects.Any(statusEffect =>
                IsMatchingStatusEffect(statusEffect, effectName));
    }

    private static bool IsMatchingStatusEffect(StatusEffect statusEffect, string effectName)
    {
        var statusName = statusEffect.m_name ?? string.Empty;
        if (statusName.IndexOf(effectName, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        var hashFieldName = effectName switch
        {
            "Wet" => "s_statusEffectWet",
            "Cold" => "s_statusEffectCold",
            "Rested" => "s_statusEffectRested",
            "Shelter" => "s_statusEffectShelter",
            _ => null
        };

        if (hashFieldName is null)
        {
            return false;
        }

        var hashField = AccessTools.Field(typeof(SEMan), hashFieldName);
        var expectedHash = hashField?.GetValue(null);
        var nameHashField = AccessTools.Field(typeof(StatusEffect), "m_nameHash");
        var actualHash = nameHashField?.GetValue(statusEffect);
        return expectedHash is not null
            && actualHash is not null
            && Convert.ToInt32(expectedHash) == Convert.ToInt32(actualHash);
    }

    private static void CharacterDamagePostfix(Character __instance, HitData hit)
    {
        if (_situationHandler is null
            || Player.m_localPlayer is null
            || __instance != Player.m_localPlayer)
        {
            return;
        }

        if (hit.m_hitType == HitData.HitType.Fall)
        {
            var fallHandled = _situationHandler.TryHandle(EmotionSituation.FallDamageTaken);
            _logger?.LogInfo($"FallDamageTaken detected. Phrase handled: {fallHandled}.");
            return;
        }

        var attacker = hit.GetAttacker();
        if (attacker is null || attacker.IsPlayer())
        {
            return;
        }

        var attackerName = LocalizeGameText(attacker.GetHoverName());
        var isDeathsquito = attacker.name.IndexOf("deathsquito", StringComparison.OrdinalIgnoreCase) >= 0
            || attackerName.IndexOf("deathsquito", StringComparison.OrdinalIgnoreCase) >= 0;
        var situation = isDeathsquito
            ? EmotionSituation.DeathsquitoAttack
            : EmotionSituation.HostileCreatureAttack;
        var context = isDeathsquito
            ? null
            : new Dictionary<string, string>
            {
                ["enemy"] = attackerName
            };

        var handled = _situationHandler.TryHandle(
            situation,
            context);
        _logger?.LogInfo($"{situation} detected from '{attackerName}'. Phrase handled: {handled}.");
    }

    private static void PlayerTeleportToPostfix(Player __instance, bool __result)
    {
        if (_situationHandler is null
            || !__result
            || __instance != Player.m_localPlayer)
        {
            return;
        }

        var handled = _situationHandler.TryHandle(EmotionSituation.PortalUsed);
        _logger?.LogInfo($"Player teleport detected. Phrase handled: {handled}.");
    }

    private static void CraftingStationInteractPostfix(
        CraftingStation __instance,
        Humanoid user,
        bool __result)
    {
        if (_situationHandler is null
            || !__result
            || user != Player.m_localPlayer)
        {
            return;
        }

        if (!IsCauldron(__instance))
        {
            return;
        }

        HandleCauldronOpened("CraftingStation.Interact");
    }

    private static void PlayerSetCraftingStationPostfix(
        Player __instance,
        CraftingStation station)
    {
        if (_situationHandler is null
            || __instance != Player.m_localPlayer
            || !IsCauldron(station))
        {
            return;
        }

        HandleCauldronOpened("Player.SetCraftingStation");
    }

    private static void HandleCauldronOpened(string detector)
    {
        var handled = _situationHandler?.TryHandle(EmotionSituation.CauldronOpened) ?? false;
        _logger?.LogInfo(
            $"CauldronOpened detected by {detector}. Phrase handled: {handled}.");
    }

    private static void InventoryGuiDoCraftingPostfix(Player player)
    {
        if (_situationHandler is null
            || player != Player.m_localPlayer
            || !IsCauldron(player.GetCurrentCraftingStation()))
        {
            return;
        }

        var handled = _situationHandler.TryHandle(EmotionSituation.CauldronCookingStarted);
        _logger?.LogInfo($"CauldronCookingStarted detected. Phrase handled: {handled}.");
    }

    private static void InventoryGuiOnCraftPressedPostfix()
    {
        var player = Player.m_localPlayer;
        if (_situationHandler is null
            || player is null
            || !IsCauldron(player.GetCurrentCraftingStation()))
        {
            return;
        }

        var handled = _situationHandler.TryHandle(EmotionSituation.CauldronCookingStarted);
        _logger?.LogInfo(
            $"CauldronCookingStarted detected by InventoryGui.OnCraftPressed. Phrase handled: {handled}.");
    }

    private static void InventoryGuiUpdateRecipePostfix(
        InventoryGui __instance,
        Player player)
    {
        if (_situationHandler is null || player != Player.m_localPlayer)
        {
            return;
        }

        var isCauldron = IsCauldron(player.GetCurrentCraftingStation());
        var craftTimerField = AccessTools.Field(typeof(InventoryGui), "m_craftTimer");
        var craftTimer = Convert.ToSingle(craftTimerField?.GetValue(__instance) ?? 0f);
        var isCrafting = craftTimer > 0;

        if (_wasCrafting && !isCrafting)
        {
            var situation = _wasCraftingAtCauldron
                ? EmotionSituation.CauldronCookingFinished
                : EmotionSituation.CraftingCompleted;
            var handled = _situationHandler.TryHandle(situation);
            _logger?.LogInfo($"{situation} detected. Phrase handled: {handled}.");
        }

        _wasCrafting = isCrafting;
        _wasCraftingAtCauldron = isCrafting && isCauldron;
        _wasCauldronCrafting = _wasCraftingAtCauldron;
    }

    private static bool IsCauldron(CraftingStation? station)
    {
        if (station is null)
        {
            return false;
        }

        return ContainsCauldron(station.m_name)
            || ContainsCauldron(station.name)
            || ContainsCauldron(station.gameObject.name);
    }

    private static bool ContainsCauldron(string? value)
    {
        return value?.IndexOf("cauldron", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void PlayerEatFoodPostfix(Player __instance, bool __result)
    {
        if (_situationHandler is null
            || !__result
            || __instance != Player.m_localPlayer)
        {
            return;
        }

        var handled = _situationHandler.TryHandle(EmotionSituation.FoodEaten);
        _logger?.LogInfo($"FoodEaten detected. Phrase handled: {handled}.");
    }

    private static void PlayerDeathPostfix(Player __instance)
    {
        if (_situationHandler is null || __instance != Player.m_localPlayer)
        {
            return;
        }

        var handled = _situationHandler.TryHandle(EmotionSituation.PlayerDeath);
        _logger?.LogInfo($"PlayerDeath detected. Phrase handled: {handled}.");
    }

    private static void PlayerRespawnPostfix(Player __instance)
    {
        if (_situationHandler is null || __instance != Player.m_localPlayer)
        {
            return;
        }

        var handled = _situationHandler.TryHandle(EmotionSituation.PlayerRespawned);
        _logger?.LogInfo($"PlayerRespawned detected. Phrase handled: {handled}.");
    }

    private static void FishPickupPostfix(
        Fish __instance,
        Humanoid character,
        bool __result)
    {
        if (_situationHandler is null
            || !__result
            || character != Player.m_localPlayer)
        {
            return;
        }

        var fishName = LocalizeGameText(
            __instance.GetHoverName().Replace("(Clone)", string.Empty));
        var handled = _situationHandler.TryHandle(
            EmotionSituation.FishingCatch,
            new Dictionary<string, string>
            {
                ["fish"] = fishName
            });
        _logger?.LogInfo($"FishingCatch detected. Phrase handled: {handled}.");
    }

    private static void TameableTamePostfix(Tameable __instance)
    {
        if (_situationHandler is null)
        {
            return;
        }

        var creature = __instance.GetComponent<Character>();
        var creatureName = creature is null
            ? LocalizeGameText(__instance.name.Replace("(Clone)", string.Empty))
            : LocalizeGameText(creature.GetHoverName());
        var handled = _situationHandler.TryHandle(
            EmotionSituation.TamedCreature,
            new Dictionary<string, string>
            {
                ["creature"] = creatureName
            });
        _logger?.LogInfo($"TamedCreature detected. Phrase handled: {handled}.");
    }

    private static void RandomEventSetActivePostfix(RandomEvent ev, bool end)
    {
        if (_situationHandler is null || end || ev is null)
        {
            return;
        }

        var eventName = ev.m_name ?? string.Empty;
        if (eventName.IndexOf("army", StringComparison.OrdinalIgnoreCase) < 0
            && eventName.IndexOf("raid", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return;
        }

        var handled = _situationHandler.TryHandle(EmotionSituation.RaidStarted);
        _logger?.LogInfo($"RaidStarted detected for '{eventName}'. Phrase handled: {handled}.");
    }

    private static void TreeBaseDamagePostfix(TreeBase __instance, HitData hit)
    {
        if (_situationHandler is null
            || Player.m_localPlayer is null
            || hit.m_hitType != HitData.HitType.Tree
            || __instance.m_health > 0f)
        {
            return;
        }

        var handled = _situationHandler.TryHandle(EmotionSituation.TreeFelled);
        _logger?.LogInfo($"TreeFelled detected. Phrase handled: {handled}.");
    }

    private static void PickableUseItemPostfix(
        Pickable __instance,
        Humanoid user,
        ItemDrop.ItemData item,
        bool __result)
    {
        HandlePickableGathered(__instance, user, __result);
    }

    private static void PickableInteractPostfix(
        Pickable __instance,
        Humanoid character,
        bool __result)
    {
        HandlePickableGathered(__instance, character, __result);
    }

    private static void HandlePickableGathered(
        Pickable pickable,
        Humanoid user,
        bool wasPicked)
    {
        if (_situationHandler is null
            || !wasPicked
            || user != Player.m_localPlayer)
        {
            return;
        }

        var plant = pickable.GetComponent<Plant>()
            ?? pickable.GetComponentInParent<Plant>();
        var situation = plant is null
            ? EmotionSituation.WildPlantGathered
            : EmotionSituation.PlantHarvested;
        var plantName = LocalizeGameText(pickable.GetHoverName());
        var handled = _situationHandler.TryHandle(
            situation,
            new Dictionary<string, string>
            {
                ["plant"] = plantName
            });
        _logger?.LogInfo($"{situation} detected for '{plantName}'. Phrase handled: {handled}.");
    }

    private static string LocalizeGameText(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || Localization.instance is null)
        {
            return text;
        }

        return Localization.instance.Localize(text);
    }

    private static void ItemStandUseItemPostfix(
        ItemStand __instance,
        Humanoid user,
        ItemDrop.ItemData item,
        bool __result)
    {
        if (_situationHandler is null
            || !__result
            || user != Player.m_localPlayer
            || __instance.GetComponentInParent<BossStone>() is null)
        {
            return;
        }

        var bossName = LocalizeGameText(
            __instance.GetComponentInParent<BossStone>()?.name
                .Replace("(Clone)", string.Empty)
            ?? "босс");
        var handled = _situationHandler.TryHandle(
            EmotionSituation.BossSummoned,
            new Dictionary<string, string>
            {
                ["boss"] = bossName
            });
        _logger?.LogInfo($"BossSummoned detected. Phrase handled: {handled}.");
    }
}
