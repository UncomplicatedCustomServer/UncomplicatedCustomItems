using Exiled.API.Features;
using Exiled.API.Features.Items;
using UncomplicatedCustomItems.API.Features.Data;
using UncomplicatedCustomItems.API.Serializable;
using UncomplicatedCustomItems.Commands.Enums;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public abstract class CustomThing
    {
        public CustomThing(Player player, ThingInfo info, SerializableThing serializableThing)
        {
            Name = serializableThing.Name;
            Description = serializableThing.Description;
            Player = player;
            Info = info;
            Serializable = serializableThing;
        }

        public string Name { get; }

        public string Description { get; }

        public ThingInfo Info { get; }

        public Player Player { get; internal set; }

        public Item Item { get; protected set; }

        protected SerializableThing Serializable { get; }

        /// <summary>
        /// Spawn custom item in hand
        /// </summary>
        public void Spawn(Vector3 position = default)
        {
            Item = Item.Create(Serializable.Model, Player);
            Item.Scale = Serializable.Scale;
            Info.Set(Item);

            if (Player is not null)
            {
                Player.CurrentItem = Item;
            }
            else
            {
                Item.CreatePickup(position, Quaternion.identity, true);
            }

            Plugin.API.Add(this);
        }

        /// <summary>
        /// Create custom item by thing type
        /// </summary>
        /// <param name="player"></param>
        /// <param name="thingType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CustomThing Create(Player player, ThingType thingType, int id)
        {
            switch (thingType)
            {
                case ThingType.Item:
                    return Plugin.Instance.Config.CustomItems[id].Create(player);
                case ThingType.Weapon:
                    return Plugin.Instance.Config.CustomWeapons[id].Create(player);
                case ThingType.Armor:
                    return Plugin.Instance.Config.CustomArmors[id].Create(player);
                case ThingType.Keycard:
                    return Plugin.Instance.Config.CustomKeycards[id].Create(player);
                default:
                    break;
            }

            return null;
        }
    }
}
