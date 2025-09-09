using System.Linq;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Components
{
    public class RegenAmmo : MonoBehaviour
    {
        private FirearmItem Item;
        private MagazineModule MagazineModule;
        private CylinderAmmoModule CylinderAmmoModule;
        private SummonedCustomItem CustomItem;
        private float RemaingRegenTime;
        private InfiniteAmmoSettings InfiniteAmmoSettings;
        public bool Paused { get; set; }

        public void Init(FirearmItem item)
        {
            Item = item;

            if (Item._ammoContainerModule is MagazineModule magazineModule)
                MagazineModule = magazineModule;

            if (Item._ammoContainerModule is CylinderAmmoModule cylinderAmmoModule)
                CylinderAmmoModule = cylinderAmmoModule;

            if (!Utilities.TryGetSummonedCustomItem(item.Serial, out var customItem))
                Destroy(this);

            CustomItem = customItem;
            InfiniteAmmoSettings infiniteAmmoSettings = CustomItem.CustomItem.FlagSettings.InfiniteAmmoSettings.FirstOrDefault();
            InfiniteAmmoSettings = infiniteAmmoSettings;
        }

        private void Update()
        {
            if (RemaingRegenTime > 0f)
            {
                RemaingRegenTime -= Time.deltaTime;
                return;
            }

            if (Paused)
                return;

            if (!Utilities.IsSummonedCustomItem(Item.Serial))
                Destroy(this);

            if (!InfiniteAmmoSettings.PassiveRegeneration)
                Destroy(this);

            if (MagazineModule is not null && MagazineModule.AmmoStored < MagazineModule.AmmoMax)
            {
                MagazineModule.ServerModifyAmmo(InfiniteAmmoSettings.RegenAmount);
                RemaingRegenTime = InfiniteAmmoSettings.RegenCoolDown;
            }
            else if (CylinderAmmoModule is not null && CylinderAmmoModule.AmmoStored < CylinderAmmoModule.AmmoMax)
            {
                CylinderAmmoModule.ServerModifyAmmo(InfiniteAmmoSettings.RegenAmount);
                RemaingRegenTime = InfiniteAmmoSettings.RegenCoolDown;
            }
        }
    }
}