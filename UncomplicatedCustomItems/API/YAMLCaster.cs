using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Interfaces.SpecificData;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UncomplicatedCustomItems.API.Features;

namespace UncomplicatedCustomItems.API
{
    internal static class YAMLCaster
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
                Log.Error($"Error while decoding class!\nError code: 0x401\nExpected key: {Expected}\nKey list: {KeyList}");
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
        /// <returns></returns>
        public static Data Decode(CustomItemType type, Dictionary<string, string> data)
        {
            return type switch
            {
                CustomItemType.Item => (Data)Decode(new ItemData(), data),
                CustomItemType.Keycard => (Data)Decode(new KeycardData(), data),
                CustomItemType.Armor => (Data)Decode(new ArmorData(), data),
                CustomItemType.Weapon => (Data)Decode(new WeaponData(), data),
                CustomItemType.Medikit => (Data)Decode(new MedikitData(), data),
                CustomItemType.Painkillers => (Data)Decode(new PainkillersData(), data),
                CustomItemType.Jailbird => (Data)Decode(new JailbirdData(), data),
                CustomItemType.ExplosiveGrenade => (Data)Decode(new ExplosiveGrenadeData(), data),
                CustomItemType.FlashGrenade => (Data)Decode(new FlashGrenadeData(), data),
                CustomItemType.Adrenaline => (Data)Decode(new AdrenalineData(), data),
                _ => new(),
            };
        }

        /// <summary>
        /// Check if the given <paramref name="data"/> (<see cref="Dictionary{string, string}"/>) can fullify the class at <paramref name="element"/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="data"></param>
        /// <param name="ExpectedKey"></param>
        /// <param name="KeyList"></param>
        /// <returns><see cref="true"/> if everything is OK</returns>
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
                Weight = item.Weight,
                Scale = item.Scale,
                CustomItemType = item.CustomItemType,
                CustomData = Decode(item.CustomItemType, item.CustomData)
            };
            return NewItem;
        }
    }
}