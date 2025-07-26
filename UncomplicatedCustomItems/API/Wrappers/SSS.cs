using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomItems.API.Features.Helper;
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

        private static ServerSpecificSettingBase CreateSettingCopy(ServerSpecificSettingBase original)
        {
            ServerSpecificSettingBase copy = ServerSpecificSettingsSync.CreateInstance(original.GetType());

            copy.SettingId = original.SettingId;
            copy.Label = original.Label;
            copy.HintDescription = original.HintDescription;
            copy.PlayerPrefsKey = original.PlayerPrefsKey;

            switch (original)
            {
                case SSTwoButtonsSetting twoButtons:
                    var twoButtonsCopy = copy as SSTwoButtonsSetting;
                    twoButtonsCopy.OptionA = twoButtons.OptionA;
                    twoButtonsCopy.OptionB = twoButtons.OptionB;
                    twoButtonsCopy.DefaultIsB = twoButtons.DefaultIsB;
                    twoButtonsCopy.SyncIsB = twoButtons.SyncIsB;
                    break;

                case SSButton button:
                    var buttonCopy = copy as SSButton;
                    buttonCopy.ButtonText = button.ButtonText;
                    buttonCopy.HoldTimeSeconds = button.HoldTimeSeconds;
                    break;

                case SSKeybindSetting keybind:
                    var keybindCopy = copy as SSKeybindSetting;
                    keybindCopy.SuggestedKey = keybind.SuggestedKey;
                    keybindCopy.PreventInteractionOnGUI = keybind.PreventInteractionOnGUI;
                    keybindCopy.AllowSpectatorTrigger = keybind.AllowSpectatorTrigger;
                    keybindCopy.AssignedKeyCode = keybind.AssignedKeyCode;
                    keybindCopy.SyncIsPressed = keybind.SyncIsPressed;
                    break;

                case SSPlaintextSetting plaintext:
                    var plaintextCopy = copy as SSPlaintextSetting;
                    plaintextCopy.Label = plaintext.Label;
                    plaintextCopy.Placeholder = plaintext.Placeholder;
                    plaintextCopy.ContentType = plaintext.ContentType;
                    plaintextCopy.CharacterLimit = plaintext.CharacterLimit;
                    plaintextCopy.SyncInputText = plaintext.SyncInputText;
                    plaintextCopy._characterLimitOriginalCache = plaintext._characterLimitOriginalCache;
                    break;

                case SSTextArea textArea:
                    var textAreaCopy = copy as SSTextArea;
                    textAreaCopy.Label = textArea.Label;
                    textAreaCopy.Foldout = textArea.Foldout;
                    textAreaCopy.AlignmentOptions = textArea.AlignmentOptions;
                    break;

                case SSGroupHeader header:
                    var headerCopy = copy as SSGroupHeader;
                    headerCopy.Label = header.Label;
                    headerCopy.ReducedPadding = header.ReducedPadding;
                    break;

                case SSDropdownSetting dropdown:
                    var dropdownCopy = copy as SSDropdownSetting;
                    dropdownCopy.Options = dropdown.Options;
                    dropdownCopy.DefaultOptionIndex = dropdown.DefaultOptionIndex;
                    dropdownCopy.EntryType = dropdown.EntryType;
                    dropdownCopy.SyncSelectionIndexRaw = dropdown.SyncSelectionIndexRaw;
                    break;

                case SSSliderSetting slider:
                    var sliderCopy = copy as SSSliderSetting;
                    sliderCopy.MinValue = slider.MinValue;
                    sliderCopy.MaxValue = slider.MaxValue;
                    sliderCopy.SyncFloatValue = slider.SyncFloatValue;
                    sliderCopy.DefaultValue = slider.DefaultValue;
                    break;
            }

            copy.ApplyDefaultValues();
            return copy;
        }

        public static void SendSettingsToUser(ReferenceHub user, ServerSpecificSettingBase[] settings)
        {
            List<ServerSpecificSettingBase> userSettings = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(user);

            foreach (ServerSpecificSettingBase setting in settings)
            {
                ServerSpecificSettingBase settingCopy = CreateSettingCopy(setting);
                if (ReferenceEquals(settings, Plugin.Instance._playerSettings))
                {
                    RemoveSettingsFromUser(user, Plugin.Instance._ToolGunSettings);
                    RemoveSettingsFromUser(user, Plugin.Instance._DebugSettings);
                }

                AddOrUpdateUserSetting(user, settingCopy);
            }

            ServerSpecificSettingsSync.DefinedSettings = userSettings.ToArray();
            ServerSpecificSettingsSync.SendToPlayer(user, userSettings.ToArray());
        }

        public static void RemoveSettingsFromUser(ReferenceHub user, ServerSpecificSettingBase[] settingsToRemove)
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return;

            HashSet<(int SettingId, Type SettingType)> settingsToRemoveSet = new HashSet<(int, Type)>();
            foreach (ServerSpecificSettingBase setting in settingsToRemove)
            {
                LogManager.Debug($"Removing {setting.Label}, {setting.SettingId}");
                settingsToRemoveSet.Add((setting.SettingId, setting.GetType()));
            }

            userSettings.RemoveAll(existingSetting => settingsToRemoveSet.Contains((existingSetting.SettingId, existingSetting.GetType())));

            ServerSpecificSettingsSync.DefinedSettings = ServerSpecificSettingsSync.DefinedSettings
                .Where(s => !settingsToRemoveSet.Contains((s.SettingId, s.GetType())))
                .ToArray();

            ServerSpecificSettingsSync.SendToPlayer(user, userSettings.ToArray());
        }

        public static void ClearAllUserSettings(ReferenceHub user)
        {
            if (ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
            {
                userSettings.Clear();
                ServerSpecificSettingsSync.DefinedSettings = new ServerSpecificSettingBase[0];
                ServerSpecificSettingsSync.SendToPlayer(user, new ServerSpecificSettingBase[0]);
            }
        }

        public static bool HasUserSetting(ReferenceHub user, int settingId, Type settingType)
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return false;

            return userSettings.Any(setting => setting.SettingId == settingId && setting.GetType() == settingType);
        }

        /// <summary>
        /// Updates a specific setting's value for a <see cref="Player"/> without recreating it
        /// </summary>
        /// <param name="user"></param>
        /// <param name="settingId"></param>
        /// <param name="settingType"></param>
        /// <param name="updateAction"></param>
        public static void UpdateUserSettingValue(ReferenceHub user, int settingId, Type settingType, Action<ServerSpecificSettingBase> updateAction)
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return;

            var setting = userSettings.FirstOrDefault(s => s.SettingId == settingId && s.GetType() == settingType);
            if (setting != null)
            {
                updateAction(setting);
                ServerSpecificSettingsSync.DefinedSettings = userSettings.ToArray();
                ServerSpecificSettingsSync.SendToPlayer(user, userSettings.ToArray());
            }
        }

        /// <summary>
        /// Gets a specific setting from a <see cref="Player"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="user"></param>
        /// <param name="settingId"></param>
        /// <returns></returns>
        public static T GetUserSetting<T>(ReferenceHub user, int settingId) where T : ServerSpecificSettingBase
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return null;

            return userSettings.FirstOrDefault(s => s.SettingId == settingId && s is T) as T;
        }
    }
}