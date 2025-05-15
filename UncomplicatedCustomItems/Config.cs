using System;
using System.ComponentModel;
namespace UncomplicatedCustomItems
{
    public class Config
    {
        [Description("Specifies whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Specifies whether developer (debug) mode is enabled.")]
        public bool Debug { get; set; } = false;

        [Description("The hint message displayed whenever you pick up a custom item. %name% is replaced with the item's name, and %desc% with its description.")]
        public string PickedUpMessage { get; set; } = "You have picked up a %name% who's a %desc%";

        [Description("The duration (in seconds) for which the hint is displayed.")]
        public float PickedUpMessageDuration { get; set; } = 3f;

        [Description("The hint message displayed whenever you select a custom item. %name% is replaced with the item's name, and %desc% with its description.")]
        public string SelectedMessage { get; set; } = "You have picked up a %name% who's a %desc%";

        [Description("If false, the UCS credit tag system will not be activated. Please do not disable it, as many contributors worked on this plugin for free.")]
        public bool EnableCreditTags { get; set; } = true;

        [Description("The duration (in seconds) for which the hint is displayed.")]
        public float SelectedMessageDuration { get; set; } = 3f;

        [Description("The hint message displayed when a player interacts with a workstation while holding a custom item with the WorkstationBan flag. Currently does nothing since LabApi dosent have a event for this")]
        public string WorkstationBanHint { get; set; } = "You are not allowed to change the attachments on %name%!";

        [Description("The duration (in seconds) for which the WorkstationBan hint is displayed. Currently does nothing since LabApi dosent have a event for this")]
        public float WorkstationBanHintDuration { get; set; } = 3f;

        [Description("Enables admin messages. Occasionally, you will receive important notifications on your console from our central servers.")]
        public bool DoEnableAdminMessages { get; set; } = true;

        [Description("Allow server tracking? (This does nothing to your server it only allows us to view the amount of servers using the plugin)")]
        public bool ServerTracking { get; set; } = true;

        [Description("Enables or disables the CommonUtilities intergration. (Set the item name in the CommonUtilities config to the customitem name)")]
        [Obsolete("Does nothing as CommonUtilities is a Exiled plugin")]
        public bool EnableCommonUtilitiesIntergration { get; set; } = true;
    }
}
