namespace UncomplicatedCustomItems.API.Features
{
    public class YAMLCustomAction
    {
        public uint Id { get; set; } = 1;
        public string Name { get; set; } = "Example";
        public string Description { get; set; } = "An example action to help you get started :)";
        public string[] Actions { get; set; } =
        [
            "if {Player.Health} < 100 then Heal {Player.UserId} 10",
            "{Player.DisplayName} = :3"
        ];
    }
}