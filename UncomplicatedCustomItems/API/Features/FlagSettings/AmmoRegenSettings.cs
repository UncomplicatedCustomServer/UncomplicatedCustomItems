namespace UncomplicatedCustomItems.API.Features
{
    public class AmmoRegenSettings
    {
        public float RegenDelay { get; set; } = 5f;
        public float RegenInterval { get; set; } = 1f;
        public int AmmoPerInterval { get; set; } = 1;
    }
}
