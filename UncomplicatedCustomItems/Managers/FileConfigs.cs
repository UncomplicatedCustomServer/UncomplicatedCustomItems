using Exiled.API.Features;
using Exiled.Loader;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomItems;
using UncomplicatedCustomItems.Elements;
using UncomplicatedCustomItems.API;
using YamlDotNet.Core.Tokens;

namespace UncomplicatedCustomItems.Manager
{
    internal class FileConfigs
    {
        internal string Dir = Path.Combine(Paths.Configs, "UncomplicatedCustomItems");

        public bool Is(string localDir = "")
        {
            return Directory.Exists(Path.Combine(Dir, localDir));
        }

        public string[] List(string localDir = "")
        {
            return Directory.GetFiles(Path.Combine(Dir, localDir));
        }

        public void LoadAll(string localDir = "")
        {
            LoadAction((YAMLCustomItem Item) =>
            {
                Utilities.Register(YAMLCaster.Converter(Item));
            }, localDir);
        }

        public void LoadAction(Action<YAMLCustomItem> action, string localDir = "")
        {
            foreach (string FileName in List(localDir))
            {
                try
                {
                    if (Directory.Exists(FileName))
                        continue;

                    if (FileName.Split().First() == ".")
                        return;

                    if (CustomTypeChecker(File.ReadAllText(FileName), out YAMLCustomItem item, out string error))
                    {
                        LogManager.Debug($"Proposed to the registerer the external item {item.Id} [{item.Name}] from file:\n{FileName}");
                        action(item);
                    }
                    else
                        LogManager.Error($"Error during the deserialization of the CustomItem at {FileName}: {error}");
                }
                catch (Exception ex)
                {

                    Utilities.NotLoadedItems.Add(new(TryGetItemId(File.ReadAllText(FileName)), FileName, ex.GetType().Name, ex.Message));

                    if (!Plugin.Instance.Config.Debug)
                        LogManager.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.");
                    else
                        LogManager.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.\nStack trace: {ex.StackTrace}");
                }
            }
        }

        private bool CustomTypeChecker(string content, out YAMLCustomItem Item, out string error)
        {
            Dictionary<string, object> data = Loader.Deserializer.Deserialize<Dictionary<string, object>>(content);
            Item = default;
            error = null;

            SnakeCaseNamingStrategy namingStrategy = new();

            foreach (PropertyInfo property in typeof(CustomItem).GetProperties().Where(p => p.CanWrite && p is not null && p.GetType() is not null))
                if (!data.ContainsKey(namingStrategy.GetPropertyName(property.Name, false)) && error is null)
                    error = $"Given CustomItem doesn't contain the required property '{namingStrategy.GetPropertyName(property.Name, false)}' ({namingStrategy.GetPropertyName(property.PropertyType.Name, false)})";

            if (error is null)
            {
                Item = Loader.Deserializer.Deserialize<YAMLCustomItem>(content);
                return true;
            }

            return false;
        }

        public void Echo() { }

        public void Welcome(string localDir = "")
        {
            if (!Is(localDir))
            {
                Directory.CreateDirectory(Path.Combine(Dir, localDir));
                File.WriteAllText(Path.Combine(Dir, localDir, "example-weapon.yml"), Loader.Serializer.Serialize(new CustomItem()
                {
                    Id = Utilities.GetFirstFreeID(1)
                }));

                LogManager.Info($"Plugin does not have a item folder, generated one in {Path.Combine(Dir, localDir)}");
            }
        }

        public static string TryGetItemId(string content)
        {
            string[] pieces = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            if (pieces.Contains("id:"))
                return pieces.FirstOrDefault(l => l.Contains("id:")).Replace(" ", "").Replace("id:", "");
            return "ND";
        }
    }
}