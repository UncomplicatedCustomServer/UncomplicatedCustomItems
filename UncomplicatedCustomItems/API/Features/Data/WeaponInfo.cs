﻿using Exiled.API.Features.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems.API.Features.Data
{
    public class WeaponInfo : ThingInfo
    {
        public float Damage { get; set; }

        public float FireRate { get; set; }

        public byte MaxAmmo { get; set; }

        public override void Set(Item item)
        {
            if (item is not Firearm firearm)
            {
                return;
            }

            firearm.FireRate = FireRate;
            firearm.MaxAmmo = MaxAmmo;
            firearm.Ammo = firearm.MaxAmmo;
        }
    }
}
