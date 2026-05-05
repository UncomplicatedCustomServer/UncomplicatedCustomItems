using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules
{
    public class TantrumOnUse : CustomModuleBase
    {
        public override string Name => "TantrumOnUse";

        public override void Run(EventArgs eventArgs)
        {
            if (!Check(eventArgs))
                return;
                
            if (eventArgs is PlayerUsedItemEventArgs ev)
            {
                Vector3 targetPosition = ev.Player.Position;
                if (Physics.Raycast(ev.Player.Position, Vector3.down, out RaycastHit hitInfo, 3f))
                    targetPosition = hitInfo.point + Vector3.up * 1.25f;

                TantrumHazard tantrum = TantrumHazard.Spawn(targetPosition, ev.Player.Rotation, new Vector3(1, 1, 1));

                foreach (TeslaGate gate in TeslaGate.AllGates)
                {
                    if (gate.IsInIdleRange(ev.Player.Position))
                        gate.TantrumsToBeDestroyed.Add(tantrum.Base);
                }
            }
        }

        public override void RegisterEvents()
        {
            PlayerEvents.UsedItem += Run;
        }

        public override void UnregisterEvents()
        {
            PlayerEvents.UsedItem -= Run;
        }
    }
}