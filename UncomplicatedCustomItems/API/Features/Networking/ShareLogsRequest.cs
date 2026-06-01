using System.Collections.Generic;
using LabApi.Features;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Features.Manager;
using System;
using System.Linq;
using System.IO;
using LabApi.Loader.Features.Paths;
using System.Text.RegularExpressions;


namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class ShareLogsRequest : APIRequest
    {
        public override string Name => nameof(ShareLogsRequest);

        public override string Endpoint => "logs/upload";

        public override RequestType Type => RequestType.Post;

        public override Dictionary<string, string> Headers { get; set; } = new()
        {
            ["Content-Type"] = "application/json"
        };

        public override Dictionary<string, object> Payload { get; set; } = new()
        {
            ["plugin_version"] = Plugin.Instance.Version.ToString(3),
            ["labapi_version"] = LabApiProperties.CurrentVersion,
            ["port"] = Server.Port,
            ["log_data"] = string.Join("\n", LogManager.History.Select(l => $"[{l.LogTime}] [{l.LogLevel}] {l.Message}")),
            ["local_log_data"] = ProcessLocalAdminLogs(),
        };

        public static bool GetLocalAdminConfig()
        {
            string portPath = Path.Combine(PathManager.SecretLab.ToString(), "config", Server.Port.ToString(), "config_localadmin.txt");
            string globalPath = Path.Combine(PathManager.SecretLab.ToString(), "config", "config_localadmin_global.txt");

            if (File.Exists(portPath))
            {
                if (IsLoggingEnabled(portPath))
                    return true;
            }

            if (File.Exists(globalPath))
            {
                if (IsLoggingEnabled(globalPath))
                    return true;
            }

            return false;
        }

        private static bool IsLoggingEnabled(string filePath)
        {
            foreach (string line in File.ReadLines(filePath))
            {
                if (line.Trim().ToLower().StartsWith("enable_la_logs: true"))
                    return true;
            }

            return false;
        }

        public static string GetLocalAdminLogs()
        {
            if (!Plugin.Instance.Config.AllowLocalAdminLogUpload || !GetLocalAdminConfig())
                return "Disabled";

            DirectoryInfo directory = new(Path.Combine(PathManager.SecretLab.ToString(), "LocalAdminLogs", Server.Port.ToString()));
            FileInfo latestFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            return latestFile?.FullName ?? string.Empty;
        }
        
        public static string ProcessLocalAdminLogs()
        {
            string logPath = GetLocalAdminLogs();
            if (string.IsNullOrEmpty(logPath) || !File.Exists(logPath))
                return "No logs found.";

            string[] Keywords =
            [
                "The referenced script on this Behaviour",
                "Trying to access a shader",
                "The referenced script (Unknown) on this Behaviour"
            ];

            string ipPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";

            try
            {
                List<string> lines = [];

                using (FileStream fs = new(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new(fs))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                            lines.Add(line);
                    }
                }

                IEnumerable<string> filteredLines = lines
                    .Where(line => !Keywords.Any(spam => line.Contains(spam)))
                    .Select(line => Regex.Replace(line, ipPattern, "[REDACTED IP]"));

                return string.Join(Environment.NewLine, filteredLines);
            }
            catch (Exception ex)
            {
                return $"Error processing logs: {ex.Message}";
            }
        }
    }
}