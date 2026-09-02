using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UncomplicatedCustomItems.API.Features.SpecificData;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Manager;
using UncomplicatedCustomItems.API.Enums;
using UncomplicatedCustomItems.API.CustomModuleAPI;
using System.Text.Json;

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
            JsonNamingPolicy namingPolicy = JsonNamingPolicy.SnakeCaseLower;

            foreach (PropertyInfo property in element.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<YamlIgnoreAttribute>() != null)
                    continue;

                string key = namingPolicy.ConvertName(property.Name);
                object? value = property.GetValue(element);

                serialized[key] = value ?? "";
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
        public static Data Decode(Data baseElement, Dictionary<string, object> data)
        {
            JsonNamingPolicy namingPolicy = JsonNamingPolicy.SnakeCaseLower;
            foreach (PropertyInfo property in baseElement.GetType().GetProperties())
            {
                string key = namingPolicy.ConvertName(property.Name);
                if (data.TryGetValue(key, out object? value))
                {
                    try
                    {
                        if (value == null)
                        {
                            SetDefaultValue(baseElement, property);
                            continue;
                        }

                        Type propType = property.PropertyType;
                        if (propType.IsEnum)
                        {
                            try
                            {
                                object enumValue = Enum.Parse(propType, value.ToString()!, ignoreCase: true);
                                property.SetValue(baseElement, enumValue);
                            }
                            catch
                            {
                                SetDefaultValue(baseElement, property);
                                LogManager.Warn($"{nameof(Decode)}: Invalid enum value '{value}' for property '{property.Name}'. Using default value.");
                            }
                        }
                        else if (propType.IsPrimitive || propType == typeof(string) || propType == typeof(decimal))
                        {
                            object convertedValue = Convert.ChangeType(value, propType);
                            property.SetValue(baseElement, convertedValue);
                        }
                        else
                        {
                            object? convertedValue = ConvertComplexValue(value, propType);
                            property.SetValue(baseElement, convertedValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetDefaultValue(baseElement, property);
                        LogManager.Warn($"{nameof(Decode)}: Failed to convert value '{value}' for property '{property.Name}': {ex.Message}. Using default value.");
                    }
                }
                else
                {
                    SetDefaultValue(baseElement, property);
                    LogManager.Debug($"{nameof(Decode)}: Property '{property.Name}' missing from data. Using default value.");
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
        private static object? ConvertComplexValue(object value, Type targetType)
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
                            object? convertedItem = yamlDeserializer.Deserialize(yamlString, elementType);
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
                {
                    property.SetValue(obj, string.Empty, null);                    
                }
                else if (property.PropertyType.IsGenericType && (property.PropertyType.GetGenericTypeDefinition() == typeof(List<>) || property.PropertyType.GetGenericTypeDefinition() == typeof(HashSet<>) || property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                {
                    object defaultValue = Activator.CreateInstance(property.PropertyType);
                    property.SetValue(obj, defaultValue, null);
                }
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
                (CustomItemType.Item, _) => Decode(new ItemData(), data),
                (CustomItemType.Keycard, _) => Decode(new KeycardData(), data),
                (CustomItemType.Armor, _) => Decode(new ArmorData(), data),
                (CustomItemType.Weapon, _) => Decode(new WeaponData(), data),
                (CustomItemType.Medikit, ItemType.Medkit) => Decode(new MedikitData(), data),
                (CustomItemType.Painkillers, ItemType.Painkillers) => Decode(new PainkillersData(), data),
                (CustomItemType.Adrenaline, ItemType.Adrenaline) => Decode(new AdrenalineData(), data),
                (CustomItemType.Jailbird, ItemType.Jailbird) => Decode(new JailbirdData(), data),
                (CustomItemType.ExplosiveGrenade, ItemType.GrenadeHE) => Decode(new ExplosiveGrenadeData(), data),
                (CustomItemType.FlashGrenade, ItemType.GrenadeFlash) => Decode(new FlashGrenadeData(), data),
                (CustomItemType.MicroHID, ItemType.MicroHID) => Decode(new MicroHIDData(), data),
                (CustomItemType.ParticleDisruptor, ItemType.ParticleDisruptor) => Decode(new ParticleDisruptorData(), data),
                (CustomItemType.Light, _) => Decode(new FlashlightData(), data),
                (CustomItemType.Candy, ItemType.SCP330) => Decode(new CandyData(), data),
                (CustomItemType.SCPItem, ItemType.SCP018) => Decode(new SCP018Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP207) => Decode(new SCP207Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP1509) => Decode(new SCP1509Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP500) => Decode(new SCP500Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP2176) => Decode(new SCP2176Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP244a) => Decode(new SCP244Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP244b) => Decode(new SCP244Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP1853) => Decode(new SCP1853Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP1576) => Decode(new SCP1576Data(), data),
                (CustomItemType.SCPItem, ItemType.GunSCP127) => Decode(new SCP127Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP1344) => Decode(new SCP1344Data(), data),
                (CustomItemType.SCPItem, ItemType.SCP268) => Decode(new SCP268Data(), data),
                (CustomItemType.SCPItem, _) => Decode(new SCPItemData(), data),

                _ => new Data(),
            };
        }

        /// <summary>
        /// Convert a basic <see cref="YAMLCustomItem"/> Item into a fullified <see cref="CustomItem"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static CustomItem Converter(YAMLCustomItem item)
        {
            CustomItem NewItem = new()
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
                CustomModules = CustomModuleManager.Decode(item.CustomModules),
                CustomItemType = item.CustomItemType,
                CustomData = Decode(item.CustomItemType, item.CustomData, item.Item)
            };

            return NewItem;
        }

        /// <summary>
        /// Convert a basic <see cref="YAMLCustomAction"/> Action into a fullified <see cref="CustomAction"/>
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static CustomAction Converter(YAMLCustomAction action)
        {
            CustomAction NewAction = new()
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