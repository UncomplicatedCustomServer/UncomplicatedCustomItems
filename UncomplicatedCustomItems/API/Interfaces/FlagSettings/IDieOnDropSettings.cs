namespace UncomplicatedCustomItems.API.Interfaces.FlagSettings
{
    internal interface IDieOnDropSettings
    {
        public abstract string? DeathMessage { get; set; }
        public abstract bool? Vaporize { get; set; }
    }
}
