using System.Collections;
using System.Text.Json.Serialization;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Yaml;
using UnityEngine.Networking;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System;
using YamlDotNet.Core;
using System.Text;
using static UnityEngine.Networking.UnityWebRequest;
using UncomplicatedCustomItems.API.Interfaces;
using System.Linq;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    public class CustomItemBackupSystem
    {
        private static string Url => "https://ucibackup.thaumiel-servers.workers.dev";
        private static string BackupDir => Path.Combine(Plugin.Instance.FileConfig.Dir, "Backups");
        public static bool Online = false;

        public static void Init()
        {
            if (Plugin.Instance.Config.BackupCode == "0")
                GenerateBackupCode();

            Player.Host.ReferenceHub.StartCoroutine(SendInitialRequest());
        }

        public static void Upload()
        {
            if (!Online || Plugin.Instance.Config.BackupCode == "0")
                return;

            Player.Host.ReferenceHub.StartCoroutine(UploadBackupData());
        }

        public static void Download()
        {
            if (!Online || Plugin.Instance.Config.BackupCode == "0")
                return;

            if (!Directory.Exists(BackupDir))
                Directory.CreateDirectory(BackupDir);

            Player.Host.ReferenceHub.StartCoroutine(GetBackupData());
        }

        private static void GenerateBackupCode()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@!_-";
            string code = string.Empty;
            Random random = new();
            for (int i = 0; i < UnityEngine.Random.Range(16, 48); i++)
                code += chars[random.Next(chars.Length)];

            Plugin.Instance.Config.BackupCode = code;
#if EXILED
            // Figure out how to save config in Exiled
#else
            Plugin.Instance.SaveConfig();
#endif
        }

        private static string ParseItems()
        {
            string content = string.Empty;
            foreach (string fileName in Plugin.Instance.FileConfig.List())
            {
                try
                {
                    string fileContent = File.ReadAllText(fileName);
                    if (!Plugin.Instance.FileConfig.IsActionFile(fileContent))
                        content += fileContent + Environment.NewLine;
                }
                catch(Exception ex)
                {
                    LogManager.Error($"Failed to run {nameof(CustomItemBackupSystem)}.{nameof(ParseItems)} {ex}");
                }
            }

            return content;
        }

        private static IEnumerator SendInitialRequest()
        {
            UnityWebRequest request = new($"{Url}");
            request.method = kHttpVerbGET;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                Online = true;

            request.Dispose();
        }

        private static IEnumerator GetBackupData()
        {
            UnityWebRequest request = new($"{Url}/download");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/yaml");
            request.SetRequestHeader("Token", $"{Plugin.Instance.Config.BackupCode}");         
            request.method = kHttpVerbGET;
            yield return request.SendWebRequest();
            
            if (request.result == Result.Success)
            {
                string FileDirectory = Path.Combine(BackupDir, $"BackupDownload_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
                if (!Directory.Exists(FileDirectory))
                    Directory.CreateDirectory(FileDirectory);

                foreach (ICustomItem item in CustomItem.List.ToArray())
                    CustomItem.Unregister(item);

                foreach (string item in request.downloadHandler.text.Split(["id:"], StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        string yamlItem = "id:" + item;
                        YAMLCustomItem customItem = YamlConfigParser.Deserializer.Deserialize<YAMLCustomItem>(yamlItem);
                        LogManager.Info($"Deserialized Backuped CustomItem {customItem.Name}");
                        File.WriteAllText(Path.Combine(FileDirectory, $"{customItem.Name}.yml"), yamlItem);
                    }
                    catch(YamlException yamlex)
                    {
                        LogManager.Warn($"Failed to Deserialize a backup CustomItem {yamlex}");
                        continue;
                    }
                }
            }
            else
                LogManager.Warn($"Download failed {request.responseCode} {request.error}");
        }

        private static IEnumerator UploadBackupData()
        {
            UnityWebRequest request = new($"{Url}/upload");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(ParseItems());   
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/yaml");
            request.SetRequestHeader("Token", $"{Plugin.Instance.Config.BackupCode}");
            request.method = kHttpVerbPOST;
            yield return request.SendWebRequest();

            if (request.result == Result.Success)
            {
                LogManager.Info($"Upload successful. Uploaded {Plugin.Instance.FileConfig.List().Length} CustomItems");
            }
            else
                LogManager.Warn($"Upload failed {request.responseCode} {request.error}");

            request.Dispose();
        }
    }
}