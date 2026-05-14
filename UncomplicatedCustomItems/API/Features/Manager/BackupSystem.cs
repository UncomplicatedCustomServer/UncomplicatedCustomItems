using LabApi.Loader.Features.Yaml;
using System.IO;
using System;

namespace UncomplicatedCustomItems.API.Features.Manager
{
    public class BackupKey
    {
        public string BackupCode { get; set; } = "0";
    }

    public class BackupSystem
    {
        internal static BackupKey? Key { get; set; }
        
#if EXILED
        private static string BackupDir => Path.Combine(Plugin.Instance.FileConfig.Dir, "Backups");
#else
        private static string BackupDir => Path.Combine(Plugin.Instance.FileConfig.Dir, "Backups");
#endif

        public static void Init()
        {
            if (!Directory.Exists(BackupDir))
                Directory.CreateDirectory(BackupDir);

            if (!File.Exists(Path.Combine(BackupDir, "key.yml")))
            {
                string content = YamlConfigParser.Serializer.Serialize(new BackupKey());
                File.WriteAllText(Path.Combine(BackupDir, "key.yml"), content);
            }

            if (Key == null)
            {
                string content = File.ReadAllText(Path.Combine(BackupDir, "key.yml"));
                BackupKey backup = YamlConfigParser.Deserializer.Deserialize<BackupKey>(content);
                Key = backup;
            }

            if (Key.BackupCode == "0")
                GenerateBackupCode();
        }

        private static void GenerateBackupCode()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@!_-";
            string code = string.Empty;
            Random random = new();
            for (int i = 0; i < UnityEngine.Random.Range(16, 48); i++)
                code += chars[random.Next(chars.Length)];

            if (Key == null)
            {
                string content = File.ReadAllText(Path.Combine(BackupDir, "key.yml"));
                BackupKey backup = YamlConfigParser.Deserializer.Deserialize<BackupKey>(content);
                Key = backup;
            }

            Key.BackupCode = code;
            File.WriteAllText(Path.Combine(BackupDir, "key.yml"), YamlConfigParser.Serializer.Serialize(Key));
        }
    }
}