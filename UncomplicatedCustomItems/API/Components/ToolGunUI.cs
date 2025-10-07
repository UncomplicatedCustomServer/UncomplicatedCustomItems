using System.Linq;
using System.Text;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.Extensions;
using UnityEngine;
using UserSettings.ServerSpecific;
using UncomplicatedCustomItems.API.ToolGun;
using UncomplicatedCustomItems.API.Features.Helper;
using PlayerRoles;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;

namespace UncomplicatedCustomItems.API.Components
{
    public class ToolGunUI : MonoBehaviour
    {
        private Player Owner;
        private APICustomItem BaseCustomItem;
        private SummonedCustomItem CustomItem;
        private StringBuilder Builder;
        private bool Paused;

        public void Init(object customItemobj)
        {
            if (customItemobj is SummonedCustomItem customItem)
            {
                CustomItem = customItem;
                Owner = customItem.Owner;
            }
            if (customItemobj is APICustomItem summonedItem)
                BaseCustomItem = summonedItem;

            Builder = new();
            Settings.GiveToPlayers();
            LogManager.Debug($"Starting ToolGun UI");
        }

        private void Update()
        {
            Builder.Clear();
            if (Paused)
                return;

            if (CustomItem != null)
            {
                if (!Utilities.TryGetSummonedCustomItem(Owner.CurrentItem.Serial, out var item) || item != CustomItem)
                    Destroy(this);

                if (!CustomItem.HasModule(CustomFlags.ToolGun))
                    Destroy(this);
            }

            if (BaseCustomItem != null)
            {
                if (!SummonedAPICustomItem.TryGet(BaseCustomItem, out var item) || item.CustomItem != BaseCustomItem)
                    Destroy(this);
                
                if (BaseCustomItem is not Features.CustomItemAPI.ToolGun)
                    Destroy(this);

                Owner = item.Owner;
            }

            if (Owner.Room == null)
                return;

            if (Owner.CurrentItem == null)
                Destroy(this);

            SSPlaintextSetting colorSetting = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(Owner.ReferenceHub, 21);
            SSTwoButtonsSetting deletionMode = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(Owner.ReferenceHub, 22);

            string deletioncolor = string.Empty;
            bool deletionbool = false;
            string DeletionMode = string.Empty;
            string room = string.Empty;

            if (Owner.CurrentItem is FirearmItem firearm)
            {
                if (deletionMode.SyncIsA)
                {
                    DeletionMode = "ADS";
                    if (firearm.Base.TryGetModule(out LinearAdsModule module) && module.AdsTarget)
                        deletionbool = true;
                    else
                        deletionbool = false;
                }
                else if (deletionMode.SyncIsB)
                {
                    DeletionMode = "Flashlight Toggle";
                    if (firearm.FlashlightEnabled)
                        deletionbool = true;
                    else
                        deletionbool = false;
                }
            }

            if (deletionbool)
                deletioncolor = "#00ff00";
            else
                deletioncolor = "#Ff0000";
                
            if (Owner.Room.Name.ToString() != "Unnamed")
                room = Owner.Room.Name.ToString();
            else
                room = Owner.Room.GameObject.name;
                
            colorSetting.SyncInputText.TryParseVector3(out Vector3 color);
            string hexcolor = color.ToHexColor();
            Builder.AppendLine($"<pos=-10em><voffset=-12.3em><color={Owner.RoleBase.RoleColor.ToHex()}>{Owner.Nickname} - {Owner.Role.GetFullName()}</color></voffset>");
            Builder.AppendLine($"<pos=-10em>{room} - <color=yellow>{Owner.Room.LocalPosition(Owner.Position)}</color>");
            Builder.AppendLine($"<pos=-10em>Primitive Color: <color={hexcolor}>{color}</color>");
            Builder.AppendLine($"<pos=-10em>Deletion Mode: {DeletionMode}");
            Builder.AppendLine($"<pos=-10em>Deleting: <color={deletioncolor}>{deletionbool}</color>");
            Owner.SendHint(Builder.ToString(), 0.5f);
        }

        public void Pause() => Paused = true;
        public void Unpause() => Paused = false;
    }
}