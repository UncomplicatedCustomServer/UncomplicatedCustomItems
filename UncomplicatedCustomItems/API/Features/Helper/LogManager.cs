using Discord;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using UncomplicatedCustomItems.API.Interfaces;
using Logger = LabApi.Features.Console.Logger;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LabApi.Features.Wrappers;
using System.Linq;
using System.Text.RegularExpressions;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    internal class LogManager
    {
        // We should store the data here
        public static readonly List<LogEntry> History = new();

        public static bool MessageSent { get; internal set; } = false;
        
        public static void Debug(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Debug.ToString(), message));
            if (Plugin.Instance.Config.Debug)
                Logger.Raw($"[DEBUG] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.Green);
        }

        public static void Info(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Info.ToString(), message));
            Logger.Info(message);
        }

        public static void Warn(string message, string error = "CS0000")
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString(), message, error));
            Logger.Warn(message);
        }

        public static void Error(string message, string error = "CS0000")
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString(), message, error));
            Logger.Error(message);
        }
        
        public static void Raw(string message, ConsoleColor color, string logLevel, string category)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), logLevel, message));
            Logger.Raw($"[{category}] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", color);
        }
        
        public static void Updater(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "Updater", message));
            Logger.Raw($"[Updater] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.Blue);
        }
        
        public static void Silent(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "SILENT", message));
            if (Plugin.Instance.Config.ShowSilentLogs)
                Logger.Raw($"[Silent] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.White);
        }

        public static void System(string message) => History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "SYSTEM", message));
        
        public static HttpStatusCode SendReport(out HttpContent content, out string readableSize)
        {
            content = null;
            readableSize = null;

            if (MessageSent || !Plugin.Instance.IsPrerelease)
            {
                return HttpStatusCode.Forbidden;
            }

            if (History.Count < 1)
            {
                return HttpStatusCode.Forbidden;
            }

            string formattedContent = FormatLogsForReport();

            int byteSize = Encoding.UTF8.GetByteCount(formattedContent);
            readableSize = byteSize switch
            {
                < 1024 => $"{byteSize} bytes",
                < 1024 * 1024 => $"{(byteSize / 1024.0):F2} KB",
                _ => $"{(byteSize / 1024.0 / 1024.0):F2} MB"
            };

            HttpStatusCode response = Plugin.HttpManager.ShareLogs(formattedContent, out content);

            if (response == HttpStatusCode.OK)
            {
                MessageSent = true;
            }

            return response;
        }

        public static async Task<(HttpStatusCode statusCode, HttpContent content, string readableSize)> SendReportAsync()
        {
            if (!Plugin.Instance.IsPrerelease && MessageSent)
            {
                return (HttpStatusCode.Forbidden, null, null);
            }

            if (History.Count < 1)
            {
                return (HttpStatusCode.Forbidden, null, null);
            }

            string formattedContent = await Task.Run(() => FormatLogsForReport()).ConfigureAwait(false);

            int byteSize = Encoding.UTF8.GetByteCount(formattedContent);
            string readableSize = byteSize switch
            {
                < 1024 => $"{byteSize} bytes",
                < 1024 * 1024 => $"{(byteSize / 1024.0):F2} KB",
                _ => $"{(byteSize / 1024.0 / 1024.0):F2} MB"
            };

            try
            {
                var result = await Plugin.HttpManager.ShareLogsAsync(formattedContent).ConfigureAwait(false);

                if (result.statusCode == HttpStatusCode.OK)
                {
                    MessageSent = true;
                }

                return (result.statusCode, result.content, readableSize);
            }
            catch (HttpRequestException ex)
            {
                Error($"HTTP error during log upload: {ex.Message}");
                return (HttpStatusCode.ServiceUnavailable, null, readableSize);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Error("Log upload timed out");
                return (HttpStatusCode.RequestTimeout, null, readableSize);
            }
            catch (Exception ex)
            {
                Error($"Unexpected error during log upload: {ex.Message}");
                return (HttpStatusCode.InternalServerError, null, readableSize);
            }
        }

        private static async Task<string> GetLocalAdminLogContentAsync()
        {
            try
            {
                string scpSlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SCP Secret Laboratory");
                string localAdminLogsPath = Path.Combine(scpSlPath, "LocalAdminLogs", Server.Port.ToString());

                if (!Directory.Exists(localAdminLogsPath))
                    return $"LocalAdmin logs directory not found at: {localAdminLogsPath}";

                string latestLogFile = Directory.GetFiles(localAdminLogsPath, "LocalAdmin Log*.txt")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(latestLogFile))
                    return "No LocalAdmin log files found in directory.";

                using FileStream fs = new(latestLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using StreamReader reader = new(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

                string content = await reader.ReadToEndAsync().ConfigureAwait(false);
                return RedactSensitiveData(content);
            }
            catch (Exception ex)
            {
                return RedactSensitiveData($"Error reading LocalAdmin log file: {ex.Message}");
            }
        }

        private static readonly Regex IpRegex = new(@"\b(?:25[0-5]|2[0-4]\d|1?\d{1,2})(?:\.(?:25[0-5]|2[0-4]\d|1?\d{1,2})){3}\b", RegexOptions.Compiled);
        private static readonly Regex ScpSlAuthTokenRegex = new(@"\b[a-zA-Z0-9_-]{8,32}-[a-zA-Z0-9_/-]{4,32}\b", RegexOptions.Compiled);
        private static readonly Regex SteamIdRegex = new(@"\b7656119\d{10}\b", RegexOptions.Compiled);

        public static string RedactSensitiveData(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            input = IpRegex.Replace(input, "[REDACTED_IP]");
            input = ScpSlAuthTokenRegex.Replace(input, "[REDACTED_TOKEN]");
            input = SteamIdRegex.Replace(input, "[REDACTED_STEAMID]");

            return input;
        }
        
        private static string FormatLogsForReport()
        {
            StringBuilder sb = new();

            sb.AppendLine("╔═══════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                    UncomplicatedCustomBots                        ║");
            sb.AppendLine("║                           Log Report                              ║");
            sb.AppendLine("╚═══════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            sb.AppendLine($"Generated: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
            sb.AppendLine($"Server Port: {Server.Port}");
            sb.AppendLine($"Total Entries: {History.Count}");

            var logSummary = History.GroupBy(h => h.Level).ToDictionary(g => g.Key, g => g.Count());

            sb.AppendLine("Log Level Summary:");
            foreach (var kvp in logSummary.OrderByDescending(x => x.Value))
                sb.AppendLine($"  • {kvp.Key}: {kvp.Value} entries");

            sb.AppendLine();

            if (History.Count > 0)
            {
                DateTimeOffset firstLog = History.First().DateTimeOffset;
                DateTimeOffset lastLog = History.Last().DateTimeOffset;
                sb.AppendLine($"Time Range: {firstLog:yyyy-MM-dd HH:mm:ss} to {lastLog:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Duration: {lastLog - firstLog}");
                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine("                              LOG ENTRIES");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine();

            var groupedLogs = History.GroupBy(h => h.Level).OrderBy(g => GetLogLevelPriority(g.Key));

            foreach (var group in groupedLogs)
            {
                sb.AppendLine($"┌─ {group.Key.ToUpper()} LOGS ({group.Count()} entries) ─");
                sb.AppendLine();

                foreach (LogEntry entry in group)
                {
                    sb.AppendLine(FormatLogEntry(entry));
                }

                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine("                            RAW LOGS");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine();

            foreach (LogEntry element in History)
                sb.AppendLine($"{element}");

            sb.AppendLine();

            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine("                         LOCALADMINLOG FILE");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine();

            string localAdminPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SCP Secret Laboratory", "LocalAdminLogs", Server.Port.ToString());
            string latestFile = Directory.Exists(localAdminPath) ? Directory.GetFiles(localAdminPath, "LocalAdmin Log*.txt").OrderByDescending(File.GetLastWriteTime).FirstOrDefault() : null;

            if (string.IsNullOrEmpty(latestFile) || !File.Exists(latestFile))
            {
                sb.AppendLine("No LocalAdmin log file found.");
            }
            else
            {
                FileInfo fileInfo = new(latestFile);
                if (fileInfo.Length > 1 * 1024 * 1024)
                {
                    sb.AppendLine($"LocalAdmin log skipped (too large: {(fileInfo.Length / 1024.0 / 1024.0):F2} MB)");
                }
                else
                {
                    try
                    {
                        using FileStream fs = new(latestFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                        using StreamReader reader = new(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                        string content = reader.ReadToEnd();
                        sb.AppendLine(RedactSensitiveData(content));
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"Error reading LocalAdmin log file: {ex.Message}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine("                           END OF REPORT");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");

            return sb.ToString();
        }

        private static string FormatLogEntry(LogEntry entry)
        {
            string timestamp = entry.DateTimeOffset.ToString("HH:mm:ss.fff");
            string errorPart = !string.IsNullOrEmpty(entry.Error) ? $"[{entry.Error}] " : "";

            return $" {timestamp} │ {errorPart}{entry.Content}";
        }
        
        private static int GetLogLevelPriority(string level)
        {
            return level.ToUpper() switch
            {
                "ERROR" => 0,
                "WARN" => 1,
                "INFO" => 2,
                "DEBUG" => 3,
                "UPDATER" => 4,
                "SYSTEM" => 5,
                "SILENT" => 6,
                _ => 7
            };
        }
    }
}
