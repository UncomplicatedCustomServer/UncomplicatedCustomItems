namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
#nullable enable
    /// <summary>
    /// The interface associated with <see cref="CustomItemType.Item"/>
    /// </summary>
    public interface IItemData : IData
    {
        public abstract ItemEvents Event { get; set; }

        public abstract string? Command { get; set; }

        public abstract float CoolDown { get; set; }

        public abstract string ConsoleMessage { get; set; }

        public abstract string BroadcastMessage { get; set; }

        public abstract ushort BroadcastDuration { get; set; }

        public abstract string HintMessage { get; set; }

        public abstract float HintDuration { get; set; }

        public abstract bool DestroyAfterUse { get; set; }
    }
}
