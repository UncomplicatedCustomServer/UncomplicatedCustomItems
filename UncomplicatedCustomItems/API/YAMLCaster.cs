using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.SpecificData;
using UncomplicatedCustomItems.API.Interfaces;
using UncomplicatedCustomItems.API.Interfaces.SpecificData;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Enums;
using Newtonsoft.Json.Serialization;

namespace UncomplicatedCustomItems.API
{
    /// <summary>
    /// Casts the YAML data from <see cref="YAMLCustomItem"/> or <see cref="YAMLCustomAction"/> files into the plugin
    /// </summary>
    public static class YAMLCaster
    {
        /// <summary>
        /// As YAML is a big shit, serialize <see cref="Data"/> elements into manageable Dictionaries for YAML
        /// </summary>
        /// <param name="element"></param>
        /// <returns>The <see cref="Dictionary{string, object}"/> of the class</returns>
        public static Dictionary<string, object> Encode(Data element)
        {
            Dictionary<string, object> serialized = [];
            SnakeCaseNamingStrategy snakeCaseStrategy = new();

            foreach (PropertyInfo property in element.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<YamlIgnoreAttribute>() != null)
                    continue;

                string yamlKey = snakeCaseStrategy.GetPropertyName(property.Name, false);
                object value = property.GetValue(element, null);

                serialized.Add(yamlKey, value ?? "");
            }

            return serialized;
        }

        /// <summary>
        /// As YAML is a big shit, decode the serialized <see cref="Dictionary{string, object}"/> into a fullified class, giving the <paramref name="baseElement"/>
        /// Missing properties will be set to their default values.
        /// </summary>
        /// <param name="baseElement"></param>
        /// <param name="data"></param>
        /// <returns>The class</returns>
        public static IData Decode(Data baseElement, Dictionary<string, object> data)
        {
            SnakeCaseNamingStrategy snakeCaseStrategy = new();

            foreach (PropertyInfo property in baseElement.GetType().GetProperties())
            {
                string yamlKey = snakeCaseStrategy.GetPropertyName(property.Name, false);

                if (data.TryGetValue(yamlKey, out object value))
                {
                    try
                    {
                        if (value == null)
                        {
                            SetDefaultValue(baseElement, property);
                            continue;
                        }

                        if (property.PropertyType.IsEnum)
                        {
                            try
                            {
                                object enumValue = Enum.Parse(property.PropertyType, value.ToString(), true);
                                property.SetValue(baseElement, enumValue, null);
                            }
                            catch
                            {
                                SetDefaultValue(baseElement, property);
                                LogManager.Warn($"{nameof(YAMLCaster)}: Invalid enum value '{value}' for property '{property.Name}'. Using default value.");
                            }
                        }
                        else if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                        {
                            object convertedValue = Convert.ChangeType(value, property.PropertyType);
                            property.SetValue(baseElement, convertedValue, null);
                        }
                        else
                        {
                            if (property.PropertyType.IsAssignableFrom(value.GetType()))
                            {
                                property.SetValue(baseElement, value, null);
                            }
                            else
                            {
                                object convertedValue = ConvertComplexValue(value, property.PropertyType);
                                property.SetValue(baseElement, convertedValue, null);
                            }
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
        /// Convert complex values with proper type handling
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">The target type</param>
        /// <returns>Converted value</returns>
        private static object ConvertComplexValue(object value, Type targetType)
        {
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = targetType.GetGenericArguments()[0];

                if (value is IList sourceList)
                {
                    IList targetList = (IList)Activator.CreateInstance(targetType);

                    foreach (object item in sourceList)
                    {
                        if (item == null)
                        {
                            targetList.Add(null);
                            continue;
                        }

                        if (elementType.IsAssignableFrom(item.GetType()))
                        {
                            targetList.Add(item);
                        }
                        else
                        {
                            ISerializer yamlSerializer = new SerializerBuilder()
                                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                                .Build();
                            IDeserializer yamlDeserializer = new DeserializerBuilder()
                                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                                .Build();

                            string yamlString = yamlSerializer.Serialize(item);
                            object convertedItem = yamlDeserializer.Deserialize(yamlString, elementType);
                            targetList.Add(convertedItem);
                        }
                    }

                    return targetList;
                }
            }

            try
            {
                ISerializer yamlSerializer = new SerializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                IDeserializer yamlDeserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();

                string yamlString = yamlSerializer.Serialize(value);
                return yamlDeserializer.Deserialize(yamlString, targetType);
            }
            catch
            {
                return Convert.ChangeType(value, targetType);
            }
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
                if (property.GetCustomAttribute<YamlIgnoreAttribute>() != null)
                    return;

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
        /// Decode the serialized <see cref="Dictionary{string, object}"/> into a fullified class by it's <see cref="CustomItemType"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Data Decode(CustomItemType type, Dictionary<string, object> data, ItemType item)
        {
            return (type, item) switch
            {
                (CustomItemType.Item, _) => (Data)Decode(new ItemData(), data),
                (CustomItemType.Keycard, _) => (Data)Decode(new KeycardData(), data),
                (CustomItemType.Armor, _) => (Data)Decode(new ArmorData(), data),
                (CustomItemType.Weapon, _) => (Data)Decode(new WeaponData(), data),
                (CustomItemType.Medikit, ItemType.Medkit) => (Data)Decode(new MedikitData(), data),
                (CustomItemType.Painkillers, ItemType.Painkillers) => (Data)Decode(new PainkillersData(), data),
                (CustomItemType.Adrenaline, ItemType.Adrenaline) => (Data)Decode(new AdrenalineData(), data),
                (CustomItemType.Jailbird, ItemType.Jailbird) => (Data)Decode(new JailbirdData(), data),
                (CustomItemType.ExplosiveGrenade, ItemType.GrenadeHE) => (Data)Decode(new ExplosiveGrenadeData(), data),
                (CustomItemType.FlashGrenade, ItemType.GrenadeFlash) => (Data)Decode(new FlashGrenadeData(), data),
                (CustomItemType.MicroHID, ItemType.MicroHID) => (Data)Decode(new MicroHIDData(), data),
                (CustomItemType.ParticleDisruptor, ItemType.ParticleDisruptor) => (Data)Decode(new ParticleDisruptorData(), data),
                (CustomItemType.Light, _) => (Data)Decode(new FlashlightData(), data),
                (CustomItemType.Candy, ItemType.SCP330) => (Data)Decode(new CandyData(), data),
                (CustomItemType.SCPItem, ItemType.SCP018) => (Data)Decode(new SCP018Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP207) => (Data)Decode(new SCP207Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP500) => (Data)Decode(new SCP500Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP2176) => (Data)Decode(new SCP2176Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP244a) => (Data)Decode(new SCP244Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP244b) => (Data)Decode(new SCP244Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP1853) => (Data)Decode(new SCP1853Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP1576) => (Data)Decode(new SCP1576Data(), data),
                (CustomItemType.SCPItem, ItemType.GunSCP127) => (Data)Decode(new SCP127Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP1344) => (Data)Decode(new SCP1344Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP268) => (Data)Decode(new SCP268Data(), data),
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
                ExtendedDescription = item.ExtendedDescription,
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

        /// <summary>
        /// Convert a basic <see cref="YAMLCustomAction"/> Action into a fullified <see cref="ICustomAction"/>
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ICustomAction Converter(YAMLCustomAction action)
        {
            ICustomAction NewAction = new CustomAction
            {
                Id = action.Id,
                Name = action.Name,
                Description = action.Description,
                Actions = action.Actions
            };

            return NewAction;
        }
    }
}