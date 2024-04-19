using Exiled.API.Features;
using System.ComponentModel;
using System.Runtime.Serialization;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Data;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Serializable
{
    public class SerializableCustomKeycard : SerializableThing
    {
        [Description("Info")]
        public KeycardInfo Info { get; set; }

        /// <summary>
        /// Return custom item by serializable 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override CustomThing Create(Player player) => new CustomKeycard(player, Info, this);
    }
}
