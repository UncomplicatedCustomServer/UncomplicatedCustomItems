using System.ComponentModel;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;

namespace UncomplicatedCustomItems.API.Features.SpecificData
{
    /// <summary>
    /// Holds the configuration data specific to custom items of type <see cref="CustomItemType.Keycard"/>.
    /// </summary>
    public class KeycardData : Data, IKeycardData
    {
        /// <summary>
        /// Sets the Containment access level. Valid range 0-3.
        /// </summary>
        [Description("Sets the Containment access level (e.g., 0-3).")]
        public virtual int Containment { get; set; } = 1;

        /// <summary>
        /// Sets the Armory access level. Valid range 0-3.
        /// </summary>
        [Description("Sets the Armory access level (e.g., 0-3).")]
        public virtual int Armory { get; set; } = 1;

        /// <summary>
        /// Sets the Admin/Management access level. Valid range 0-3.
        /// </summary>
        [Description("Sets the Admin access level (e.g., 0-3).")]
        public virtual int Admin { get; set; } = 1;

        /// <summary>
        /// Sets the main background color of the keycard. Use hex color codes (e.g., #FFD700).
        /// </summary>
        [Description("Sets the main color of the keycard. Use hex codes (e.g., #FFD700).")]
        public virtual string TintColor { get; set; } = "#FFD700";

        /// <summary>
        /// Sets the color of the permission indicators on the keycard. Use hex color codes (e.g., #FFD700).
        /// </summary>
        [Description("Sets the color of the permissions text/indicators on the keycard. Use hex codes.")]
        public virtual string PermissionsColor { get; set; } = "#FFD700";

        /// <summary>
        /// Sets the primary name displayed on the keycard (e.g., the holder's name).
        /// </summary>
        [Description("Sets the name displayed on the keycard.")]
        public virtual string Name { get; set; } = "%name%";

        /// <summary>
        /// Sets the secondary label displayed on the keycard (e.g., a title or department).
        /// </summary>
        [Description("Sets the label displayed on the keycard.")]
        public virtual string Label { get; set; } = "Hello :D";

        /// <summary>
        /// Sets the serial number displayed visually on the keycard. Digit amount should be 12 Note: This is distinct from the underlying item's unique serial ID.
        /// </summary>
        [Description("Sets the serial number displayed on the keycard. Digit amount should be 12! This is different from the item's internal serial!")]
        public virtual string SerialNumber { get; set; } = "123456789012";

        /// <summary>
        /// This is currently unused or doesnt get applied by custom keycards
        /// </summary>
        [Description("This is currently unused or doesnt get applied by custom keycard.")]
        public virtual byte WearDetail { get; set; } = 1;

        /// <summary>
        /// Sets the color of the label text on the keycard. Use hex color codes (e.g., #FFD700).
        /// </summary>
        [Description("Sets the color of the label text on the keycard. Use hex codes.")]
        public virtual string LabelColor { get; set; } = "#FFD700";

        /// <summary>
        /// This is currently unused or doesnt get applied by custom keycards
        /// </summary>
        [Description("This is currently unused or doesnt get applied by custom keycard.")]
        public virtual int Rank { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether this keycard should be consumed after a single successful use.
        /// </summary>
        [Description("Specifies if the keycard is consumed after one use.")]
        public virtual bool OneTimeUse { get; set; } = false;

        /// <summary>
        /// The hint message displayed to the user after this keycard is consumed (if OneTimeUse is true). %name% is replaced with the keycard's Name.
        /// </summary>
        [Description("The hint shown after the keycard is consumed (if OneTimeUse is true). Use %name% for the keycard's name.")]
        public virtual string OneTimeUseHint { get; set; } = "%name% Was a one time use keycard!";
    }
}