namespace UncomplicatedCustomItems.API.Interfaces.FlagSettings
{
    internal interface IDieOnUseSettings
    {
        public abstract string? DeathMessage { get; set; }
        public abstract bool? Vaporize { get; set; }
    }
}
