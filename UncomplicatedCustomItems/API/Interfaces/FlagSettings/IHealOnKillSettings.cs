namespace UncomplicatedCustomItems.API.Interfaces
{
    public interface IHealOnKillSettings
    {
        float? HealAmount { get; set; }
        bool? ConvertToAhpIfFull { get; set; }
    }
}