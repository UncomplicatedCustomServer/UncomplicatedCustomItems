using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Manager;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class Capybara : CustomModuleBase
    {
        public override string Name => "Capybara";
        public static Dictionary<Player, CapybaraToy> capybaras = [];

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            LogManager.Debug($"Running Capybara");
            switch (eventArgs)
            {
                case PlayerDroppedItemEventArgs ev:
                    DestroyCapy(ev.Player);
                    break;

                case PlayerDeathEventArgs ev:
                    DestroyCapy(ev.Player);
                    break;

                case PlayerItemUsageEffectsApplyingEventArgs ev:
                    CreateCapy(ev.Player);
                    break;

                case PlayerPickedUpItemEventArgs ev:
                    CreateCapy(ev.Player);
                    break;
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.DroppedItem += Run;
            PlayerEvents.ItemUsageEffectsApplying += Run;
            PlayerEvents.PickedUpItem += Run;
            PlayerEvents.Death += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.DroppedItem -= Run;
            PlayerEvents.ItemUsageEffectsApplying -= Run;
            PlayerEvents.PickedUpItem -= Run;
            PlayerEvents.Death -= Run;
        }

        private void CreateCapy(Player player)
        {
            if (capybaras.ContainsKey(player))
                return;
            
            LogManager.Debug($"Spawned Capybara on {player.DisplayName}");
            CapybaraToy capybara = CapybaraToy.Create(player.GameObject?.transform);
            capybara.CollidersEnabled = false;
            capybara.Position += new Vector3(0, -0.8f, 0);
            player.Scale = new(0.2f, 0.3f, 0.5f);
            capybara.Scale = new(6f, 4f, 2.8f);
            capybara.GameObject.name += "UCI";
            player.EnableEffect<Fade>(255, float.MaxValue);
            capybaras.TryAdd(player, capybara);
        } 

        private void DestroyCapy(Player player)
        {
            if (!capybaras.ContainsKey(player))
                return;

            CapybaraToy toy = capybaras[player];
            toy.Destroy();
            player.DisableEffect<Fade>();
            player.Scale = new(1f, 1f, 1f);
            capybaras.Remove(player);
        }
    }
}