using System;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.Enums;

namespace UncomplicatedCustomItems.API
{
    /// <summary>
    /// Casts the YAML data from <see cref="CustomItem"/> files into the plugin
    /// </summary>
    public static class YAMLCaster
    {
        /// <summary>
        /// As YAML is a big shit, serialize <see cref="Data"/> elements into manageable Dictionaries for YAML
        /// </summary>
        /// <param name="element"></param>
        /// <returns>The <see cref="Dictionary{string, string}"/> of the class</returns>
        public static Dictionary<string, string> Encode(Data element)
        {
            Dictionary<string, string> serialized = new();
            foreach (PropertyInfo Property in element.GetType().GetProperties())
            {
                //Log.Debug($"Encoding class {element.GetType().FullName} >> Property {Property.Name} as {Property.GetValue(element, null)}");
                SnakeCaseNamingStrategy snakeCaseStrategy = new();
                serialized.Add(snakeCaseStrategy.GetPropertyName(Property.Name, false), (Property.GetValue(element, null) ?? "error").ToString());
            }
            return serialized;
        }

        /// <summary>
        /// As YAML is a big shit, decode the serialized <see cref="Dictionary{string, string}"/> into a fullified class, giving the <paramref name="baseElement"/>
        /// </summary>
        /// <param name="baseElement"></param>
        /// <param name="data"></param>
        /// <returns>The class</returns>
        public static IData Decode(Data baseElement, Dictionary<string, string> data)
        {
            if (!Check(baseElement, data, out string Expected, out string KeyList))
            {
                LogManager.Error($"Error while decoding class!\nError code: 0x401\nExpected key: {Expected}\nKey list: {KeyList}");
                return new Data();
            }

            foreach (KeyValuePair<string, string> Elements in data)
            {
                PropertyInfo PropertyInfo = baseElement.GetType().GetProperty(PascalCaseNamingConvention.Instance.Apply(Elements.Key));

                if (PropertyInfo?.PropertyType.IsEnum ?? false)
                {
                    PropertyInfo?.SetValue(baseElement, Enum.Parse(PropertyInfo.PropertyType, Elements.Value), null);
                } 
                else
                {
                    PropertyInfo?.SetValue(baseElement, Convert.ChangeType(Elements.Value, PropertyInfo.PropertyType), null);
                }
            }

            return baseElement;
        }

        /// <summary>
        /// Decode the serialized <see cref="Dictionary{string, string}"/> into a fullified class by it's <see cref="CustomItemType"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Data Decode(CustomItemType type, Dictionary<string, string> data, ItemType item)
        {
            return (type, item) switch
            {
                (CustomItemType.Item, _) => (Data)Decode(new ItemData(), data),
                (CustomItemType.Keycard, _) => (Data)Decode(new KeycardData(), data),
                (CustomItemType.Armor, _) => (Data)Decode(new ArmorData(), data),
                (CustomItemType.Weapon, _) => (Data)Decode(new WeaponData(), data),
                (CustomItemType.Medikit, _) => (Data)Decode(new MedikitData(), data),
                (CustomItemType.Painkillers, _) => (Data)Decode(new PainkillersData(), data),
                (CustomItemType.Jailbird, _) => (Data)Decode(new JailbirdData(), data),
                (CustomItemType.ExplosiveGrenade, _) => (Data)Decode(new ExplosiveGrenadeData(), data),
                (CustomItemType.FlashGrenade, _) => (Data)Decode(new FlashGrenadeData(), data),
                (CustomItemType.Adrenaline, _) => (Data)Decode(new AdrenalineData(), data),
                (_, ItemType.SCP018) => (Data)Decode(new SCP018Data(), data),
                (_, ItemType.SCP207) => (Data)Decode(new SCP207Data(), data),
                (_, ItemType.SCP500) => (Data)Decode(new SCP500Data(), data),
                (_, ItemType.SCP330) => (Data)Decode(new SCP330Data(), data),
                (_, ItemType.SCP2176) => (Data)Decode(new SCP2176Data(), data),
                (_, ItemType.SCP244a) => (Data)Decode(new SCP244Data(), data),
                (_, ItemType.SCP244b) => (Data)Decode(new SCP244Data(), data),
                (_, ItemType.SCP1853) => (Data)Decode(new SCP1853Data(), data),
                (_, ItemType.SCP1576) => (Data)Decode(new SCP1576Data(), data),
                (CustomItemType.SCPItem, _) => (Data)Decode(new SCPItemData(), data),

                _ => new Data(),
            };
        }


        /// <summary>
        /// Check if the given <paramref name="data"/> (<see cref="Dictionary{string, string}"/>) can fullify the class at <paramref name="element"/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="data"></param>
        /// <param name="ExpectedKey"></param>
        /// <param name="KeyList"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if everything is OK</returns>
        public static bool Check(IData element, Dictionary<string, string> data, out string ExpectedKey, out string KeyList)
        {
            ExpectedKey = null;
            KeyList = null;

            SnakeCaseNamingStrategy snakeCaseStrategy = new();

            foreach (PropertyInfo Property in element.GetType().GetProperties())
            {
                if (!data.ContainsKey(snakeCaseStrategy.GetPropertyName(Property.Name, false)))
                {
                    ExpectedKey = snakeCaseStrategy.GetPropertyName(Property.Name, false);
                    KeyList = string.Join(", ", data.Keys);
                    return false;
                }
            }

            return true;
        }
        

        /// <summary>
        /// Convert a basic <see cref="YAMLCustomItem"/> Item into a fullified <see cref="ICustomItem"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ICustomItem Converter(YAMLCustomItem item)
        {
            ICustomItem NewItem = new CustomItem
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Item = item.Item,
                BadgeName = item.BadgeName,
                BadgeColor = item.BadgeColor,
                Weight = item.Weight,
                Scale = item.Scale,
                Spawn = item.Spawn,
                CustomFlags = item.CustomFlags,
                FlagSettings = item.FlagSettings,
                CustomItemType = item.CustomItemType,
                CustomData = Decode(item.CustomItemType, item.CustomData, item.Item)
            };
            return NewItem;
        }
    }
}

