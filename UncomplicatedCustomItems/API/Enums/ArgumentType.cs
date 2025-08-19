using LabApi.Events.Arguments.PlayerEvents;
using UncomplicatedCustomItems.API.Attributes;

namespace UncomplicatedCustomItems.API.Enums
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum ArgumentType
    {
        [EventType(typeof(PlayerJoinedEventArgs))]
        OnJoined,

        [EventType(typeof(PlayerLeftEventArgs))]
        OnLeft,

        [EventType(typeof(PlayerReceivingVoiceMessageEventArgs))]
        OnReceivingVoiceMessage,

        [EventType(typeof(PlayerSendingVoiceMessageEventArgs))]
        OnSendingVoiceMessage,

        [EventType(typeof(PlayerPreAuthenticatingEventArgs))]
        OnPreAuthenticating,

        [EventType(typeof(PlayerPreAuthenticatedEventArgs))]
        OnPreAuthenticated,

        [EventType(typeof(PlayerUsingIntercomEventArgs))]
        OnUsingIntercom,

        [EventType(typeof(PlayerUsedIntercomEventArgs))]
        OnUsedIntercom,

        [EventType(typeof(PlayerBanningEventArgs))]
        OnBanning,

        [EventType(typeof(PlayerBannedEventArgs))]
        OnBanned,

        [EventType(typeof(PlayerKickingEventArgs))]
        OnKicking,

        [EventType(typeof(PlayerKickedEventArgs))]
        OnKicked,

        [EventType(typeof(PlayerMutingEventArgs))]
        OnMuting,

        [EventType(typeof(PlayerMutedEventArgs))]
        OnMuted,

        [EventType(typeof(PlayerUnmutingEventArgs))]
        OnUnmuting,

        [EventType(typeof(PlayerUnmutedEventArgs))]
        OnUnmuted,

        [EventType(typeof(PlayerReportingCheaterEventArgs))]
        OnReportingCheater,

        [EventType(typeof(PlayerReportedCheaterEventArgs))]
        OnReportedCheater,

        [EventType(typeof(PlayerReportingPlayerEventArgs))]
        OnReportingPlayer,

        [EventType(typeof(PlayerReportedPlayerEventArgs))]
        OnReportedPlayer,

        [EventType(typeof(PlayerTogglingNoclipEventArgs))]
        OnTogglingNoclip,

        [EventType(typeof(PlayerToggledNoclipEventArgs))]
        OnToggledNoclip,

        [EventType(typeof(PlayerRequestingRaPlayerListEventArgs))]
        OnRequestingRaPlayerList,

        [EventType(typeof(PlayerRequestedRaPlayerListEventArgs))]
        OnRequestedRaPlayerList,

        [EventType(typeof(PlayerRaPlayerListAddingPlayerEventArgs))]
        OnRaPlayerListAddingPlayer,

        [EventType(typeof(PlayerRaPlayerListAddedPlayerEventArgs))]
        OnRaPlayerListAddedPlayer,

        [EventType(typeof(PlayerRequestedCustomRaInfoEventArgs))]
        OnRequestedCustomRaInfo,

        [EventType(typeof(PlayerRequestingRaPlayersInfoEventArgs))]
        OnRequestingRaPlayersInfo,

        [EventType(typeof(PlayerRequestedRaPlayersInfoEventArgs))]
        OnRequestedRaPlayersInfo,

        [EventType(typeof(PlayerRequestingRaPlayerInfoEventArgs))]
        OnRequestingRaPlayerInfo,

        [EventType(typeof(PlayerRequestedRaPlayerInfoEventArgs))]
        OnRequestedRaPlayerInfo,

        [EventType(typeof(PlayerChangingBadgeVisibilityEventArgs))]
        OnChangingBadgeVisibility,

        [EventType(typeof(PlayerChangedBadgeVisibilityEventArgs))]
        OnChangedBadgeVisibility,

        [EventType(typeof(PlayerChangingNicknameEventArgs))]
        OnChangingNickname,

        [EventType(typeof(PlayerChangedNicknameEventArgs))]
        OnChangedNickname,

        [EventType(typeof(PlayerGroupChangingEventArgs))]
        OnGroupChanging,

        [EventType(typeof(PlayerGroupChangedEventArgs))]
        OnGroupChanged,

        [EventType(typeof(PlayerEffectUpdatingEventArgs))]
        OnUpdatingEffect,

        [EventType(typeof(PlayerEffectUpdatedEventArgs))]
        OnUpdatedEffect,

        [EventType(typeof(PlayerDyingEventArgs))]
        OnDying,

        [EventType(typeof(PlayerDeathEventArgs))]
        OnDeath,

        [EventType(typeof(PlayerHurtingEventArgs))]
        OnHurting,

        [EventType(typeof(PlayerHurtEventArgs))]
        OnHurt,

        [EventType(typeof(PlayerChangingRoleEventArgs))]
        OnChangingRole,

        [EventType(typeof(PlayerChangedRoleEventArgs))]
        OnChangedRole,

        [EventType(typeof(PlayerCuffingEventArgs))]
        OnCuffing,

        [EventType(typeof(PlayerCuffedEventArgs))]
        OnCuffed,

        [EventType(typeof(PlayerUncuffingEventArgs))]
        OnUncuffing,

        [EventType(typeof(PlayerUncuffedEventArgs))]
        OnUncuffed,

        [EventType(typeof(PlayerReceivingLoadoutEventArgs))]
        OnReceivingLoadout,

        [EventType(typeof(PlayerReceivedLoadoutEventArgs))]
        OnReceivedLoadout,

        [EventType(typeof(PlayerSpawningEventArgs))]
        OnSpawning,

        [EventType(typeof(PlayerSpawnedEventArgs))]
        OnSpawned,

        [EventType(typeof(PlayerChangingItemEventArgs))]
        OnChangingItem,

        [EventType(typeof(PlayerChangedItemEventArgs))]
        OnChangedItem,

        [EventType(typeof(PlayerDroppingAmmoEventArgs))]
        OnDroppingAmmo,

        [EventType(typeof(PlayerDroppedAmmoEventArgs))]
        OnDroppedAmmo,

        [EventType(typeof(PlayerDroppingItemEventArgs))]
        OnDroppingItem,

        [EventType(typeof(PlayerDroppedItemEventArgs))]
        OnDroppedItem,

        [EventType(typeof(PlayerPickingUpAmmoEventArgs))]
        OnPickingUpAmmo,

        [EventType(typeof(PlayerPickedUpAmmoEventArgs))]
        OnPickedUpAmmo,

        [EventType(typeof(PlayerPickingUpArmorEventArgs))]
        OnPickingUpArmor,

        [EventType(typeof(PlayerPickedUpArmorEventArgs))]
        OnPickedUpArmor,

        [EventType(typeof(PlayerPickingUpItemEventArgs))]
        OnPickingUpItem,

        [EventType(typeof(PlayerPickedUpItemEventArgs))]
        OnPickedUpItem,

        [EventType(typeof(PlayerPickingUpScp330EventArgs))]
        OnPickingUpScp330,

        [EventType(typeof(PlayerPickedUpScp330EventArgs))]
        OnPickedUpScp330,

        [EventType(typeof(PlayerSearchedAmmoEventArgs))]
        OnSearchedAmmo,

        [EventType(typeof(PlayerSearchingArmorEventArgs))]
        OnSearchingArmor,

        [EventType(typeof(PlayerSearchedArmorEventArgs))]
        OnSearchedArmor,

        [EventType(typeof(PlayerSearchingPickupEventArgs))]
        OnSearchingPickup,

        [EventType(typeof(PlayerInteractedToyEventArgs))]
        OnInteractedToy,

        [EventType(typeof(PlayerSearchedPickupEventArgs))]
        OnSearchedPickup,

        [EventType(typeof(PlayerSearchingAmmoEventArgs))]
        OnSearchingAmmo,

        [EventType(typeof(PlayerThrowingItemEventArgs))]
        OnThrowingItem,

        [EventType(typeof(PlayerThrewItemEventArgs))]
        OnThrewItem,

        [EventType(typeof(PlayerThrowingProjectileEventArgs))]
        OnThrowingProjectile,

        [EventType(typeof(PlayerThrewProjectileEventArgs))]
        OnThrewProjectile,

        [EventType(typeof(PlayerInspectingKeycardEventArgs))]
        OnInspectingKeycard,

        [EventType(typeof(PlayerInspectedKeycardEventArgs))]
        OnInspectedKeycard,

        [EventType(typeof(PlayerSpinningRevolverEventArgs))]
        OnSpinningRevolver,

        [EventType(typeof(PlayerSpinnedRevolverEventArgs))]
        OnSpinnedRevolver,

        [EventType(typeof(PlayerToggledDisruptorFiringModeEventArgs))]
        OnToggledDisruptorFiringMode,

        [EventType(typeof(PlayerUsingItemEventArgs))]
        OnUsingItem,

        [EventType(typeof(PlayerUsedItemEventArgs))]
        OnUsedItem,

        [EventType(typeof(PlayerItemUsageEffectsApplyingEventArgs))]
        OnItemUsageEffectsApplying,

        [EventType(typeof(PlayerUsingRadioEventArgs))]
        OnUsingRadio,

        [EventType(typeof(PlayerUsedRadioEventArgs))]
        OnUsedRadio,

        [EventType(typeof(PlayerAimedWeaponEventArgs))]
        OnAimedWeapon,

        [EventType(typeof(PlayerDryFiringWeaponEventArgs))]
        OnDryFiringWeapon,

        [EventType(typeof(PlayerDryFiredWeaponEventArgs))]
        OnDryFiredWeapon,

        [EventType(typeof(PlayerUnloadingWeaponEventArgs))]
        OnUnloadingWeapon,

        [EventType(typeof(PlayerUnloadedWeaponEventArgs))]
        OnUnloadedWeapon,

        [EventType(typeof(PlayerReloadingWeaponEventArgs))]
        OnReloadingWeapon,

        [EventType(typeof(PlayerReloadedWeaponEventArgs))]
        OnReloadedWeapon,

        [EventType(typeof(PlayerShootingWeaponEventArgs))]
        OnShootingWeapon,

        [EventType(typeof(PlayerShotWeaponEventArgs))]
        OnShotWeapon,

        [EventType(typeof(PlayerChangingAttachmentsEventArgs))]
        OnChangingAttachments,

        [EventType(typeof(PlayerChangedAttachmentsEventArgs))]
        OnChangedAttachments,

        [EventType(typeof(PlayerSendingAttachmentsPrefsEventArgs))]
        OnSendingAttachmentsPrefs,

        [EventType(typeof(PlayerSentAttachmentsPrefsEventArgs))]
        OnSentAttachmentsPrefs,

        [EventType(typeof(PlayerCancellingUsingItemEventArgs))]
        OnCancellingUsingItem,

        [EventType(typeof(PlayerCancelledUsingItemEventArgs))]
        OnCancelledUsingItem,

        [EventType(typeof(PlayerChangingRadioRangeEventArgs))]
        OnChangingRadioRange,

        [EventType(typeof(PlayerChangedRadioRangeEventArgs))]
        OnChangedRadioRange,

        [EventType(typeof(PlayerProcessingJailbirdMessageEventArgs))]
        OnProcessingJailbirdMessage,

        [EventType(typeof(PlayerProcessedJailbirdMessageEventArgs))]
        OnProcessedJailbirdMessage,

        [EventType(typeof(PlayerTogglingFlashlightEventArgs))]
        OnTogglingFlashlight,

        [EventType(typeof(PlayerToggledFlashlightEventArgs))]
        OnToggledFlashlight,

        [EventType(typeof(PlayerTogglingWeaponFlashlightEventArgs))]
        OnTogglingWeaponFlashlight,

        [EventType(typeof(PlayerToggledWeaponFlashlightEventArgs))]
        OnToggledWeaponFlashlight,

        [EventType(typeof(PlayerTogglingRadioEventArgs))]
        OnTogglingRadio,

        [EventType(typeof(PlayerToggledRadioEventArgs))]
        OnToggledRadio,

        [EventType(typeof(PlayerJumpedEventArgs))]
        OnJumped,

        [EventType(typeof(PlayerMovementStateChangedEventArgs))]
        OnMovementStateChanged,

        [EventType(typeof(PlayerDamagingShootingTargetEventArgs))]
        OnDamagingShootingTarget,

        [EventType(typeof(PlayerDamagedShootingTargetEventArgs))]
        OnDamagedShootingTarget,

        [EventType(typeof(PlayerDamagingWindowEventArgs))]
        OnDamagingWindow,

        [EventType(typeof(PlayerDamagedWindowEventArgs))]
        OnDamagedWindow,

        [EventType(typeof(PlayerEnteringPocketDimensionEventArgs))]
        OnEnteringPocketDimension,

        [EventType(typeof(PlayerEnteredPocketDimensionEventArgs))]
        OnEnteredPocketDimension,

        [EventType(typeof(PlayerLeavingPocketDimensionEventArgs))]
        OnLeavingPocketDimension,

        [EventType(typeof(PlayerLeftPocketDimensionEventArgs))]
        OnLeftPocketDimension,

        [EventType(typeof(PlayerTriggeringTeslaEventArgs))]
        OnTriggeringTesla,

        [EventType(typeof(PlayerTriggeredTeslaEventArgs))]
        OnTriggeredTesla,

        [EventType(typeof(PlayerEscapingEventArgs))]
        OnEscaping,

        [EventType(typeof(PlayerEscapedEventArgs))]
        OnEscaped,

        [EventType(typeof(PlayerFlippingCoinEventArgs))]
        OnFlippingCoin,

        [EventType(typeof(PlayerFlippedCoinEventArgs))]
        OnFlippedCoin,

        [EventType(typeof(PlayerSearchingToyEventArgs))]
        OnSearchingToy,

        [EventType(typeof(PlayerSearchedToyEventArgs))]
        OnSearchedToy,

        [EventType(typeof(PlayerSearchToyAbortedEventArgs))]
        OnSearchToyAborted,

        [EventType(typeof(PlayerIdlingTeslaEventArgs))]
        OnIdlingTesla,

        [EventType(typeof(PlayerIdledTeslaEventArgs))]
        OnIdledTesla,

        [EventType(typeof(PlayerInteractingDoorEventArgs))]
        OnInteractingDoor,

        [EventType(typeof(PlayerInteractedDoorEventArgs))]
        OnInteractedDoor,

        [EventType(typeof(PlayerInteractingElevatorEventArgs))]
        OnInteractingElevator,

        [EventType(typeof(PlayerInteractedElevatorEventArgs))]
        OnInteractedElevator,

        [EventType(typeof(PlayerInteractingGeneratorEventArgs))]
        OnInteractingGenerator,

        [EventType(typeof(PlayerInteractedGeneratorEventArgs))]
        OnInteractedGenerator,

        [EventType(typeof(PlayerOpeningGeneratorEventArgs))]
        OnOpeningGenerator,

        [EventType(typeof(PlayerOpenedGeneratorEventArgs))]
        OnOpenedGenerator,

        [EventType(typeof(PlayerActivatingGeneratorEventArgs))]
        OnActivatingGenerator,

        [EventType(typeof(PlayerActivatedGeneratorEventArgs))]
        OnActivatedGenerator,

        [EventType(typeof(PlayerDeactivatingGeneratorEventArgs))]
        OnDeactivatingGenerator,

        [EventType(typeof(PlayerDeactivatedGeneratorEventArgs))]
        OnDeactivatedGenerator,

        [EventType(typeof(PlayerUnlockingGeneratorEventArgs))]
        OnUnlockingGenerator,

        [EventType(typeof(PlayerUnlockedGeneratorEventArgs))]
        OnUnlockedGenerator,

        [EventType(typeof(PlayerClosingGeneratorEventArgs))]
        OnClosingGenerator,

        [EventType(typeof(PlayerClosedGeneratorEventArgs))]
        OnClosedGenerator,

        [EventType(typeof(PlayerInteractingLockerEventArgs))]
        OnInteractingLocker,

        [EventType(typeof(PlayerInteractedLockerEventArgs))]
        OnInteractedLocker,

        [EventType(typeof(PlayerInteractingScp330EventArgs))]
        OnInteractingScp330,

        [EventType(typeof(PlayerInteractedScp330EventArgs))]
        OnInteractedScp330,

        [EventType(typeof(PlayerInteractingShootingTargetEventArgs))]
        OnInteractingShootingTarget,

        [EventType(typeof(PlayerInteractedShootingTargetEventArgs))]
        OnInteractedShootingTarget,

        [EventType(typeof(PlayerPlacingBloodEventArgs))]
        OnPlacingBlood,

        [EventType(typeof(PlayerPlacedBloodEventArgs))]
        OnPlacedBlood,

        [EventType(typeof(PlayerPlacingBulletHoleEventArgs))]
        OnPlacingBulletHole,

        [EventType(typeof(PlayerPlacedBulletHoleEventArgs))]
        OnPlacedBulletHole,

        [EventType(typeof(PlayerSpawningRagdollEventArgs))]
        OnSpawningRagdoll,

        [EventType(typeof(PlayerSpawnedRagdollEventArgs))]
        OnSpawnedRagdoll,

        [EventType(typeof(PlayerUnlockingWarheadButtonEventArgs))]
        OnUnlockingWarheadButton,

        [EventType(typeof(PlayerUnlockedWarheadButtonEventArgs))]
        OnUnlockedWarheadButton,

        [EventType(typeof(PlayerReceivedAchievementEventArgs))]
        OnReceivedAchievement,

        [EventType(typeof(PlayerRoomChangedEventArgs))]
        OnRoomChanged,

        [EventType(typeof(PlayerZoneChangedEventArgs))]
        OnZoneChanged,

        [EventType(typeof(PlayerInteractingWarheadLeverEventArgs))]
        OnInteractingWarheadLever,

        [EventType(typeof(PlayerInteractedWarheadLeverEventArgs))]
        OnInteractedWarheadLever,

        [EventType(typeof(PlayerChangedSpectatorEventArgs))]
        OnChangedSpectator,

        [EventType(typeof(PlayerEnteringHazardEventArgs))]
        OnEnteringHazard,

        [EventType(typeof(PlayerEnteredHazardEventArgs))]
        OnEnteredHazard,

        [EventType(typeof(PlayersStayingInHazardEventArgs))]
        OnStayingInHazard,

        [EventType(typeof(PlayerLeavingHazardEventArgs))]
        OnLeavingHazard,

        [EventType(typeof(PlayerLeftHazardEventArgs))]
        OnLeftHazard,
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
