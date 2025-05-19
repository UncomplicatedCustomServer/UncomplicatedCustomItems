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
                settingCopy.SetId(setting.SettingId, setting.Label ?? "Unnamed");
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
            ServerSpecificSettingBase[] filtered = ServerSpecificSettingsSync.DefinedSettings.Where(s => !Plugin.Instance._ToolGunSettings.Any(def => def.SettingId == s.SettingId && def.GetType() == s.GetType() && def.Label == s.Label)).ToArray();
            ServerSpecificSettingsSync.SendToPlayer(user, filtered);
        }
    }
}