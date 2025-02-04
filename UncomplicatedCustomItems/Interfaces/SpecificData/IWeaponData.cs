using Exiled.API.Enums;
using System.Collections.Generic;
using InventorySystem.Items.Firearms.Attachments;

namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
#nullable enable
    public interface IWeaponData : IData
    {
        public abstract float Damage { get; set; }

        public abstract byte MaxBarrelAmmo { get; set; }

        public abstract byte MaxAmmo { get; set; }

        public abstract byte MaxMagazineAmmo { get; set; }

        public abstract int AmmoDrain { get; set; }

        public abstract float Penetration { get; set; }

        public abstract float Inaccuracy { get; set; }

        public abstract float DamageFalloffDistance { get; set; }
        
        public abstract AttachmentName Attachments { get; set; }
    }

}