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

namespace UncomplicatedCustomItems.API.Components
{
    public class ToolGunUI : MonoBehaviour
    {
        private Player Owner;
        private SummonedCustomItem CustomItem;
        private StringBuilder Bulder;
        private bool Paused;

        public void Init(SummonedCustomItem customItem)
        {
            CustomItem = customItem;
            Owner = customItem.Owner;
            Bulder = new();
        }

        private void Update()
        {
            if (Paused)
                return;

            if (Owner.Room == null)
                return;

            if (!Utilities.TryGetSummonedCustomItem(Owner.CurrentItem.Serial, out var item) || item != CustomItem)
                Destroy(this);

            if (Owner.CurrentItem == null)
                Destroy(this);

            if (!CustomItem.HasModule(CustomFlags.ToolGun))
                Destroy(this);

            SSPlaintextSetting colorSetting = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(Owner.ReferenceHub, 21);
            SSTwoButtonsSetting deletionMode = ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(Owner.ReferenceHub, 22);

            string deletioncolor = string.Empty;
            bool deletionbool = false;
            string DeletionMode = string.Empty;
            string room = string.Empty;
            if (Owner.CurrentItem is FirearmItem firearm)
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

            if (deletionbool)
                deletioncolor = "#00ff00";
            else
                deletioncolor = "#Ff0000";
            if (Owner.Room.Name.ToString() != "Unnamed")
                room = Owner.Room.Name.ToString();
            else
                room = Owner.Room.GameObject.name;
                
            StringExtensions.TryParseVector3(colorSetting.SyncInputText, out Vector3 color);
            string hexcolor = Vector3Extensions.ToHexColor(color);
            string hinttext = $"<pos=-10em><voffset=-12.3em><color=Red>{Owner.Nickname} - {Owner.Role.GetFullName()}</color></voffset>\n<pos=-10em>{room} - <color=yellow>{Owner.Room.LocalPosition(Owner.Position)}</color>\n<pos=-10em>Primitive Color: <color={hexcolor}>{color}</color>\n<pos=-10em>Deletion Mode: {DeletionMode}\n<pos=-10em>Deleting: <color={deletioncolor}>{deletionbool}</color>";
            Owner.SendHint($"<align=left>{hinttext}</align>", 0.5f);
        }

        public void Pause() => Paused = true;
        public void Unpause() => Paused = false;
    }
}