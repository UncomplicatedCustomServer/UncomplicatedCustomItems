namespace UncomplicatedCustomItems.API.Features.CustomItemAPI
{
    public abstract class CustomKeycard : BaseCustomItem
    {
        /// <summary>
        /// Sets the Containment access level. Valid range 0-3.
        /// </summary>
        public abstract int Containment { get; set; }

        /// <summary>
        /// Sets the Armory access level. Valid range 0-3.
        /// </summary>
        public abstract int Armory { get; set; }

        /// <summary>
        /// Sets the Admin/Door access level. Valid range 0-3.
        /// </summary>
        public abstract int Admin { get; set; }

        /// <summary>
        /// Sets the main background color of the keycard. Use hex color codes (e.g., #FFD700).
        /// </summary>
        public abstract string TintColor { get; set; }

        /// <summary>
        /// Sets the color of the permission indicators on the keycard. Use hex color codes (e.g., #FFD700).
        /// </summary>
        public abstract string PermissionsColor { get; set; }

        /// <summary>
        /// Sets the primary name displayed on the keycard. %name% will be the first users name
        /// </summary>
        public abstract string HolderName { get; set; }

        /// <summary>
        /// Sets the secondary label displayed on the keycard (e.g., a title or department).
        /// </summary>
        public virtual string Label { get; set; } = "Hello :D";

        /// <summary>
        /// Sets the serial number displayed visually on the keycard. 
        /// Only applies to <see cref="ItemType.KeycardCustomTaskForce"/> and <see cref="ItemType.KeycardCustomMetalCase"/>.
        /// </summary>
        public virtual string SerialNumber { get; set; } = "123456789012";

        /// <summary>
        /// Gets or sets the wear on the keycard.
        /// </summary>
        public virtual byte WearDetail { get; set; } = 1;

        /// <summary>
        /// Sets the color of the label text on the keycard. Use hex color codes (e.g., #FFD700).
        /// </summary>
        public virtual string LabelColor { get; set; } = "#FFD700";

        /// <summary>
        /// Gets or sets the rank on <see cref="ItemType.KeycardCustomTaskForce"/> Valid range 0-3.
        /// </summary>
        public virtual int Rank { get; set; } = 1;

        /// <summary>
        /// Gets or sets wether the <see cref="CustomKeycard"/> instance is a one time use.
        /// </summary>
        public virtual bool OneTimeUse { get; set; } = false;

        /// <summary>
        /// Gets or sets the content of the hint shown when the <see cref="CustomKeycard"/> instance is used.
        /// %name% will be replaced with the name of the <see cref="CustomKeycard"/> instance
        /// </summary>
        public virtual string OneTimeUseMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the duration of time that the one time use hint is shown.
        /// </summary>
        public virtual float OneTimeUseMessageDuration { get; set; } = 4f;
    }
}