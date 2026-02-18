using LabApi.Loader.Features.Yaml;
using UnityEngine.Networking;
using System.IO;
using System;
using YamlDotNet.Core;
using System.Text;
using static UnityEngine.Networking.UnityWebRequest;
using UncomplicatedCustomItems.API.Interfaces;
using System.Collections.Generic;
using MEC;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    public class BackupKey
    {
        public string BackupCode { get; set; } = "0";
    }

    public class CustomItemBackupSystem
    {
        private static BackupKey key { get; set; }
        private static string Url => "https://ucibackup.thaumiel-servers.workers.dev";
#if EXILED
        private static string BackupDir => Path.Combine(Plugin.Instance.FileConfig.Dir, "Backups");
#else
        private static string BackupDir => Path.Combine(Plugin.Instance.FileConfig.Dir, "Backups");
#endif
        public static bool Online = false;

        public static void Init()
        {
            if (!Directory.Exists(BackupDir))
                Directory.CreateDirectory(BackupDir);

            if (!File.Exists(Path.Combine(BackupDir, "key.yml")))
            {
                string content = YamlConfigParser.Serializer.Serialize(new BackupKey());
                File.WriteAllText(Path.Combine(BackupDir, "key.yml"), content);
            }

            if (key is null)
            {
                string content = File.ReadAllText(Path.Combine(BackupDir, "key.yml"));
                BackupKey backup = YamlConfigParser.Deserializer.Deserialize<BackupKey>(content);
                key = backup;
            }

            if (key.BackupCode == "0")
                GenerateBackupCode();

            Timing.RunCoroutine(SendInitialRequest());
        }

        public static void Upload()
        {
            if (!Online || key.BackupCode == "0")
                return;

            Timing.RunCoroutine(UploadBackupData());
        }

        public static void Download(string code = "")
        {
            if (!Online || Plugin.Instance.Config.BackupCode == "0")
                return;

            Timing.RunCoroutine(GetBackupData(code));
        }

        private static void GenerateBackupCode()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@!_-";
            string code = string.Empty;
            Random random = new();
            for (int i = 0; i < UnityEngine.Random.Range(16, 48); i++)
                code += chars[random.Next(chars.Length)];

            key.BackupCode = code;
            File.WriteAllText(Path.Combine(BackupDir, "key.yml"), YamlConfigParser.Serializer.Serialize(key));
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

        private static IEnumerator<float> SendInitialRequest()
        {
            UnityWebRequest request = new($"{Url}");
            request.method = kHttpVerbGET;
            yield return Timing.WaitUntilDone(request.SendWebRequest());
            if (request.result == Result.Success)
                Online = true;

            request.Dispose();
        }

        private static IEnumerator<float> GetBackupData(string code = "")
        {
            string BackupCode = code;
            if (string.IsNullOrEmpty(BackupCode))
                BackupCode = key.BackupCode;

            UnityWebRequest request = new($"{Url}/download");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/yaml");
            request.SetRequestHeader("Token", $"{BackupCode}");         
            request.method = kHttpVerbGET;
            yield return Timing.WaitUntilDone(request.SendWebRequest());
            
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
                        LogManager.Info($"Wrote CustomItem backup {customItem.Name} to {Path.Combine(FileDirectory, $"{customItem.Name}.yml")}");
                        File.WriteAllText(Path.Combine(FileDirectory, $"{customItem.Name}.yml"), yamlItem);
                    }
                    catch(YamlException yamlex)
                    {
                        LogManager.Warn($"Failed to Deserialize a CustomItem backup {yamlex}");
                        continue;
                    }
                }
            }
            else
                LogManager.Warn($"Download failed {request.responseCode} {request.error}");

            request.Dispose();
        }

        private static IEnumerator<float> UploadBackupData()
        {
            UnityWebRequest request = new($"{Url}/upload");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(ParseItems());   
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/yaml");
            request.SetRequestHeader("Token", $"{key.BackupCode}");
            request.method = kHttpVerbPOST;
            yield return Timing.WaitUntilDone(request.SendWebRequest());

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