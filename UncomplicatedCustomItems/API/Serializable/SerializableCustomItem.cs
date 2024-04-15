using Exiled.API.Features;
using System.ComponentModel;
using UncomplicatedCustomItems.API.Features;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Serializable
{
    public class SerializableCustomItem
    {
        [Description("Name")]
        public string Name { get; set; }

        [Description("Description")]
        public string Description { get; set; }

        [Description("Use response")]
        public int Id { get; set; }

        [Description("Model")]
        public ItemType Model { get; set; }

        [Description("Scale")]
        public Vector3 Scale { get; set; }

        [Description("Command to execute")]
        public string Command { get; set; }

        [Description("Use response")]
        public string Response { get; set; }

        /// <summary>
        /// Return custom item by serializable 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public CustomItem Create(Player player)
        {
            return new CustomItem(player, Id, this);
        }
    }
}
