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

        [Description("If true the UCS credit tag system won't be activated. PLEASE DON'T DEACTIVATE IT as LOTS OF PEOPLE WORKED ON THIS PLUGIN completly for FREE!")]
        public bool EnableCreditTags { get; set; } = true;

        [Description("The duration of that hint")]
        public float SelectedMessageDuration { get; set; } = 3f;

        [Description("The hint message that will appear when a player interacts with a workstation while holding a customitem with the WorkstationBan flag")]
        public string WorkstationBanHint { get; set; } = "You are not allowed to change the attachments on this weapon!";

        [Description("The duration of the WorkstationBan hint")]
        public float WorkstationBanDuration { get; set; } = 3f;
        [Description("Do enable the Admin Messages? You will sometimes receive messages to your console from our Central Servers (only for very important things!)")]
        public bool DoEnableAdminMessages { get; set; } = true;
    }
}