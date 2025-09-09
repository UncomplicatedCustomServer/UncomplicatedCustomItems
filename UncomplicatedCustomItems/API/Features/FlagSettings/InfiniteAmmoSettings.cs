using UncomplicatedCustomItems.API.Interfaces.FlagSettings;

namespace UncomplicatedCustomItems.API.Features
{
    public class InfiniteAmmoSettings : IInfiniteAmmoSettings
    {
        public bool RegenOnShot { get; set; }
        public bool PassiveRegeneration { get; set; }
        public float PostFireCooldown { get; set; }
        public int RegenAmount { get; set; }
        public float RegenCoolDown { get; set; }
    }
}