using System;
using System.Collections.Generic;
using System.IO;
using LabApi.Loader.Features.Yaml;
using UncomplicatedCustomItems.API.Features.Helper;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine.Networking;
using YamlDotNet.Core;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class BackupDownloadRequest : APIRequest
    {
#if EXILED
        private static string BackupDir => Path.Combine(Plugin.Instance.FileConfig.Dir, "Backups");
#else
        private static string BackupDir => Path.Combine(Plugin.Instance.FileConfig.Dir, "Backups");
#endif

        public override string Endpoint => "backup/download";

        public override RequestType Type => RequestType.Get;

        public override Dictionary<string, string> Headers { get; set; } = new()
        {
            ["Content-Type"] = "application/yaml",
            ["Token"] = BackupSystem.Key.BackupCode
        };

        public override void OnRequestCompleted(UnityWebRequest request)
        {
            string FileDirectory = Path.Combine(BackupDir, $"BackupDownload_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
            if (!Directory.Exists(FileDirectory))
                Directory.CreateDirectory(FileDirectory);

            foreach (ICustomItem item in CustomItem.List.ToArray())
            {
                CustomItem.Unregister(item);
            }

            foreach (string item in request.downloadHandler.text.Split(["id:"], StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    string yamlItem = "id:" + item;
                    YAMLCustomItem customItem = YamlConfigParser.Deserializer.Deserialize<YAMLCustomItem>(yamlItem);
                    LogManager.Info($"Wrote CustomItem backup {customItem.Name} to {Path.Combine(FileDirectory, $"{customItem.Name}.yml")}");
                    File.WriteAllText(Path.Combine(FileDirectory, $"{customItem.Name}.yml"), yamlItem);
                }
                catch (YamlException yamlex)
                {
                    LogManager.Warn($"Failed to Deserialize a CustomItem backup {yamlex}");
                    continue;
                }
            }
        }             
    }
}
