using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Data;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Serializable
{
    public abstract class SerializableThing
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

        [Description("Spawn properties")]
        public ItemSpawnPoint SpawnPoint { get; set; }

        public abstract CustomThing Create(Player player);
    }
}
