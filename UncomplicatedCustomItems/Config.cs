using Exiled.API.Interfaces;
using System.ComponentModel;
namespace UncomplicatedCustomItems
{
    public class Config : IConfig
    {
        [Description("Is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Do enable the developer (debug) mode?")]
        public bool Debug { get; set; } = true;

        [Description("The hint message that will appear every time that you pick up a custom item. %name% is the item's name, %desc% is the item's description")]
        public string PickedUpMessage { get; set; } = "You have picked up a %name% who's a %desc%";

        [Description("The duration of that hint")]
        public float PickedUpMessageDuration { get; set; } = 3f;

        [Description("The hint message that will appear every time that you select a custom item. %name% is the item's name, %desc% is the item's description")]
        public string SelectedMessage { get; set; } = "You have picked up a %name% who's a %desc%";

        [Description("The duration of that hint")]
        public float SelectedMessageDuration { get; set; } = 3f;

        [Description("Do enable the Admin Messages? You will sometimes receive messages to your console from our Central Servers (only for very important things!)")]
        public bool DoEnableAdminMessages { get; set; } = true;
    }
}