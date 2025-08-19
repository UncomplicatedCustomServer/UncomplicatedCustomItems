using System;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Enums;

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
        /// Missing properties will be set to their default values.
        /// </summary>
        /// <param name="baseElement"></param>
        /// <param name="data"></param>
        /// <returns>The class</returns>
        public static IData Decode(Data baseElement, Dictionary<string, string> data)
        {
            SnakeCaseNamingStrategy snakeCaseStrategy = new();

            foreach (PropertyInfo property in baseElement.GetType().GetProperties())
            {
                string yamlKey = snakeCaseStrategy.GetPropertyName(property.Name, false);
                
                if (data.TryGetValue(yamlKey, out string value))
                {
                    try
                    {
                        if (property.PropertyType.IsEnum)
                        {
                            try
                            {
                                object enumValue = Enum.Parse(property.PropertyType, value, true);
                                property.SetValue(baseElement, enumValue, null);
                            }
                            catch
                            {
                                SetDefaultValue(baseElement, property);
                                LogManager.Warn($"{nameof(YAMLCaster)}: Invalid enum value '{value}' for property '{property.Name}'. Using default value.");
                            }
                        }
                        else
                        {
                            object convertedValue = Convert.ChangeType(value, property.PropertyType);
                            property.SetValue(baseElement, convertedValue, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetDefaultValue(baseElement, property);
                        LogManager.Warn($"{nameof(YAMLCaster)}: Failed to convert value '{value}' for property '{property.Name}': {ex.Message}. Using default value.");
                    }
                }
                else
                {
                    SetDefaultValue(baseElement, property);
                    LogManager.Debug($"{nameof(YAMLCaster)}: Property '{property.Name}' missing from YAML data. Using default value.");
                }
            }

            return baseElement;
        }

        /// <summary>
        /// Sets a property to its default value based on the property type
        /// </summary>
        /// <param name="obj">The object instance</param>
        /// <param name="property">The property to set</param>
        private static void SetDefaultValue(object obj, PropertyInfo property)
        {
            try
            {
                if (property.PropertyType.IsValueType)
                {
                    object defaultValue = Activator.CreateInstance(property.PropertyType);
                    property.SetValue(obj, defaultValue, null);
                }
                else if (property.PropertyType == typeof(string))
                    property.SetValue(obj, string.Empty, null);
                else
                    property.SetValue(obj, null, null);
            }
            catch (Exception ex)
            {
                LogManager.Error($"{nameof(YAMLCaster)}: Failed to set default value for property '{property.Name}': {ex.Message}");
            }
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
                (CustomItemType.MicroHID, _) => (Data)Decode(new MicroHIDData(), data),
                (CustomItemType.ParticleDisruptor, _) => (Data)Decode(new ParticleDisruptorData(), data),
                (CustomItemType.Light, _) => (Data)Decode(new FlashlightData(), data),
                (_, ItemType.SCP018) => (Data)Decode(new SCP018Data(), data),
                (_, ItemType.SCP207) => (Data)Decode(new SCP207Data(), data),
                (_, ItemType.SCP500) => (Data)Decode(new SCP500Data(), data),
                (_, ItemType.SCP330) => (Data)Decode(new SCP330Data(), data),
                (_, ItemType.SCP2176) => (Data)Decode(new SCP2176Data(), data),
                (_, ItemType.SCP244a) => (Data)Decode(new SCP244Data(), data),
                (_, ItemType.SCP244b) => (Data)Decode(new SCP244Data(), data),
                (_, ItemType.SCP1853) => (Data)Decode(new SCP1853Data(), data),
                (_, ItemType.SCP1576) => (Data)Decode(new SCP1576Data(), data),
                (_, ItemType.GunSCP127) => (Data)Decode(new SCP127Data(), data),
                (_, ItemType.SCP1344) => (Data)Decode(new SCP1344Data(), data),
                (CustomItemType.SCPItem, _) => (Data)Decode(new SCPItemData(), data),

                _ => new Data(),
            };
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
                Arguments = item.Arguments,
                CustomFlags = item.CustomFlags,
                FlagSettings = item.FlagSettings,
                CustomItemType = item.CustomItemType,
                CustomData = Decode(item.CustomItemType, item.CustomData, item.Item)
            };
            return NewItem;
        }
    }
}