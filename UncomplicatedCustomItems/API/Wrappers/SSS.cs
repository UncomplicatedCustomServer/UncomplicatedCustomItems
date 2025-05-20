using LabApi.Features.Wrappers;
using System.Collections.Generic;
using System.Linq;
using UserSettings.ServerSpecific;

namespace UncomplicatedCustomItems.API.Wrappers
{
    internal static class SSS
    {
        public static void AddOrUpdateUserSetting(ReferenceHub user, ServerSpecificSettingBase setting)
        {
            List<ServerSpecificSettingBase> userSettings = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(user);

            for (int i = 0; i < userSettings.Count; i++)
            {
                if (userSettings[i].SettingId == setting.SettingId && userSettings[i].GetType() == setting.GetType())
                {
                    userSettings[i] = setting;
                    return;
                }
            }

            userSettings.Add(setting);
        }

        public static void AddToolGunSettingsToUser(ReferenceHub user)
        {
            List<ServerSpecificSettingBase> userSettings = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(user);

            foreach (ServerSpecificSettingBase setting in Plugin.Instance._ToolGunSettings)
            {
                ServerSpecificSettingBase settingCopy = ServerSpecificSettingsSync.CreateInstance(setting.GetType());
                settingCopy.SetId(setting.SettingId, setting.Label ?? "ERROR: Unknown");
                settingCopy.Label = setting.Label;
                settingCopy.HintDescription = setting.HintDescription;
                settingCopy.ApplyDefaultValues();

                AddOrUpdateUserSetting(user, settingCopy);
            }

            ServerSpecificSettingBase[] merged = ServerSpecificSettingsSync.DefinedSettings.Where(s => !Plugin.Instance._ToolGunSettings.Any(def => def.SettingId == s.SettingId && def.GetType() == s.GetType() && def.Label == s.Label)).Concat(Plugin.Instance._ToolGunSettings).ToArray();
            ServerSpecificSettingsSync.SendToPlayer(user, merged);
        }

        public static void SendNormalSettingsToUser(ReferenceHub user)
        {
            Player.TryGet(user.gameObject, out Player player);
            if (player.UserId == "76561199150506472@steam")
                AddDebugSettingsToUser(user);
            else
            {
                var excludedSettings = Plugin.Instance._ToolGunSettings.Concat(Plugin.Instance._DebugSettings);
                ServerSpecificSettingBase[] filtered = ServerSpecificSettingsSync.DefinedSettings.Where(s => !excludedSettings.Any(def => def.SettingId == s.SettingId && def.GetType() == s.GetType() && def.Label == s.Label)).ToArray();
                ServerSpecificSettingsSync.SendToPlayer(user, filtered);
            }
        }

        public static void AddDebugSettingsToUser(ReferenceHub user)
        {
            List<ServerSpecificSettingBase> userSettings = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(user);

            foreach (ServerSpecificSettingBase setting in Plugin.Instance._DebugSettings)
            {
                ServerSpecificSettingBase settingCopy = ServerSpecificSettingsSync.CreateInstance(setting.GetType());
                settingCopy.SetId(setting.SettingId, setting.Label ?? "ERROR: Unknown");
                settingCopy.Label = setting.Label;
                settingCopy.HintDescription = setting.HintDescription;
                settingCopy.ApplyDefaultValues();

                AddOrUpdateUserSetting(user, settingCopy);
            }

            ServerSpecificSettingBase[] merged = ServerSpecificSettingsSync.DefinedSettings.Where(s => !Plugin.Instance._DebugSettings.Any(def => def.SettingId == s.SettingId && def.GetType() == s.GetType() && def.Label == s.Label) && !Plugin.Instance._ToolGunSettings.Any(def => def.SettingId == s.SettingId && def.GetType() == s.GetType() && def.Label == s.Label)).Concat(Plugin.Instance._DebugSettings).ToArray();
            ServerSpecificSettingsSync.SendToPlayer(user, merged);
        }
    }
}