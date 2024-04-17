using Exiled.API.Features;
using Exiled.API.Features.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomItems.Commands.Enums;

namespace UncomplicatedCustomItems.API.Features
{
    public abstract class CustomThing
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract Item Item { get; protected set; }

        public abstract void Spawn();

        public static CustomThing Create(Player player, ThingType thingType, int id)
        {
            switch (thingType)
            {
                case ThingType.Item:
                    return Plugin.Instance.Config.CustomItems[id].Create(player);
                case ThingType.Weapon:
                    return Plugin.Instance.Config.CustomWeapons[id].Create(player);
            }

            return null;
        }
    }
}
