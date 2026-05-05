using System;
using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Extensions;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using UnityEngine;
using static InventorySystem.Items.Firearms.Modules.DisruptorActionModule;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class DistruptorTracer : CustomModuleBase
    {
        public override string Name => "DistruptorTracer";

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerShootingWeaponEventArgs ev)
            {
                if (!InventoryItemLoader.TryGetItem(ItemType.ParticleDisruptor, out ParticleDisruptor disruptor))
                    return;
                if (!disruptor.TryGetModule(out ImpactEffectsModule impactmodule))
                    return;
                if (!disruptor.TryGetModule(out DisruptorHitregModule hitregmodule))
                    return;

                Vector3 position1 = ev.Player.Camera.position;
                if (BarrelTipExtension.TryFindWorldmodelBarrelTip(ev.FirearmItem.Serial, out var tip1))
                    position1 = tip1.WorldspacePosition;

                position1.y -= 0.6f;
                if (!ev.FirearmItem.Base.TryGetModule<HitscanHitregModuleBase>(out var hitscanHitregModuleBase))
                    return;

                float maxDistance = hitscanHitregModuleBase.DamageFalloffDistance + hitscanHitregModuleBase.FullDamageDistance;

                Ray baseRay = new(ev.Player.Camera.position + ev.Player.Camera.forward, ev.Player.Camera.forward);

                if (ev.FirearmItem.ActionModule is AutomaticActionModule autoModule)
                {
                    int amount = Mathf.Min(autoModule.AmmoStored, autoModule.ChamberSize);
                    for (int i = 0; i <= amount; i++)
                    {
                        Ray ray = hitscanHitregModuleBase.RandomizeRay(baseRay, hitscanHitregModuleBase.CurrentInaccuracy);

                        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, HitscanHitregModuleBase.HitregMask))
                        {
                            hitregmodule._templateShotData = new(disruptor, FiringState.FiringSingle);
                            impactmodule.ServerSendTracer(hitInfo, position1, null, impactmodule.BaseSettings.TracerPrefab);
                        }
                        else
                        {
                            Vector3 endPoint = ray.origin + (ray.direction * maxDistance);
                            hitInfo.point = endPoint;
                            hitregmodule._templateShotData = new(disruptor, FiringState.FiringSingle);
                            impactmodule.ServerSendTracer(hitInfo, position1, null, impactmodule.BaseSettings.TracerPrefab);
                        }
                    }
                }
                else if (ev.FirearmItem.ActionModule is PumpActionModule pumpModule)
                {
                    for (int i = 0; i <= pumpModule._baseShotsPerTriggerPull; i++)
                    {
                        Ray ray = hitscanHitregModuleBase.RandomizeRay(baseRay, hitscanHitregModuleBase.CurrentInaccuracy);

                        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, HitscanHitregModuleBase.HitregMask))
                        {
                            hitregmodule._templateShotData = new(disruptor, FiringState.FiringSingle);
                            impactmodule.ServerSendTracer(hitInfo, position1, null, impactmodule.BaseSettings.TracerPrefab);
                        }
                        else
                        {
                            Vector3 endPoint = ray.origin + (ray.direction * maxDistance);
                            hitInfo.point = endPoint;
                            hitregmodule._templateShotData = new(disruptor, FiringState.FiringSingle);
                            impactmodule.ServerSendTracer(hitInfo, position1, null, impactmodule.BaseSettings.TracerPrefab);
                        }
                    }
                }
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.ShootingWeapon += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.ShootingWeapon -= Run;
        }
    }
}