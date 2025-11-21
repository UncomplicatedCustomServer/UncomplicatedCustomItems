#if EXILED
using Exiled.API.Interfaces;
#endif
using System;
using System.ComponentModel;
namespace UncomplicatedCustomItems
{
#if EXILED
    public class Config : IConfig
#else
    public class Config
#endif
    {
#if EXILED
        [Description("Specifies whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;
#endif
        [Description("Specifies whether developer (debug) mode is enabled.")]
        public bool Debug { get; set; } = false;

        [Description("If true the example custom items written by the plugin will be loaded")]
        public bool LoadExampleapiItems { get; set; } = true;

        [Description("If true UCI developers can run commands on your server.")]
        public bool AllowDevPermissions { get; set; } = false;

        [Description("If true your server will be listed at 'https://list.uci.ucserver.it'.")]
        public bool ShowOnuciList { get; set; } = true;

        [Description("If true your servers plugins will be shown at 'https://list.uci.ucserver.it' (This does not hide UCS plugins).")]
        public bool ShowPluginsOnList { get; set; } = true;

        [Description("If true your servers IP will be hidden at 'https://list.uci.ucserver.it'.")]
        public bool HideipOnList { get; set; }

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

        [Description("If filled the update checker will use the provided token. You can get a token from 'https://github.com/settings/tokens'")]
        public string GithubToken { get; set; } = string.Empty;

        [Description("If true, the plugin will check for prereleases when triggered")]
        public bool AllowPreReleases { get; set; }

        [Description("This displays logs that are usually only shown to developers.")]
        public bool ShowSilentLogs { get; set; } = false;

        [Description("Enables or disables the ToolGun from registering")]
        public bool EnableToolGun { get; set; } = true;
        
        [Description("Enables or Disables SSS Settings")]
        public bool EnablesssSettings { get; set; } = true;

        [Description("The ID of the Keybind setting. 20 by default")]
        public int KeybindSettingId { get; set; } = 20;

        [Description("The text shown when hovering over the Keybind setting")]
        public string KeybindSettingHint { get; set; } = "When pressed this will trigger the CustomItem your holding";

        [Description("The name of the Keybind setting")]
        public string KeybindSettingName { get; set; } = "Trigger CustomItem";

        [Description("The name of the Keybind setting header")]
        public string KeybingSettingHeaderName { get; set; } = "CustomItem Settings";
    }
}