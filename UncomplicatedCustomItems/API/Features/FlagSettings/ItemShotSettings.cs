using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class ItemShotSettings
    {
        public bool IsGrenade { get; set; }
        public bool GrenadeExplodeOnImpact { get; set; }
        public bool IsCustomItem { get; set; }
        public float Velocity { get; set; }
        public float UpwardsFactor { get; set; }
        public Vector3 Torque { get; set; }
        public uint CustomItemId { get; set; }
        public ItemType ItemType { get; set; }
    }
}