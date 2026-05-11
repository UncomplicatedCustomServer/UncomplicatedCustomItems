using Discord;
using System;
using System.Collections.Generic;
using System.Net;
using Logger = LabApi.Features.Console.Logger;
using System.Text;
using System.IO;
using LabApi.Features.Wrappers;
using System.Linq;
using System.Text.RegularExpressions;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    internal class LogManager
    {
        public class Log
        {
            public Log(DateTime logTime, LogLevel level, string message)
            {
                LogLevel = level;
                Message = message;
                LogTime = logTime;
            }

            public LogLevel LogLevel { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime LogTime { get; set; }
        }

        // We should store the data here
        public static readonly List<Log> History = [];

        public static bool MessageSent { get; internal set; }
        
        public static void Debug(string message)
        {
            History.Add(new(DateTime.Now, LogLevel.Debug, message));
            if (Plugin.Instance.Config.Debug)
                Logger.Raw($"[DEBUG] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.Green);
        }

        public static void Info(string message)
        {
            History.Add(new(DateTime.Now, LogLevel.Info, message));
            Logger.Info(message);
        }

        public static void Warn(string message, string error = "CS0000")
        {
            History.Add(new(DateTime.Now, LogLevel.Warn, message));
            Logger.Warn(message);
        }

        public static void Error(string message, string error = "CS0000")
        {
            History.Add(new(DateTime.Now, LogLevel.Error, message));
            Logger.Error(message);
        }
        
        public static void Raw(string message, ConsoleColor color, LogLevel logLevel, string category)
        {
            History.Add(new(DateTime.Now, logLevel, message));
            Logger.Raw($"[{category}] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", color);
        }
        
        public static void Updater(string message)
        {
            History.Add(new(DateTime.Now, LogLevel.Info, message));
            Logger.Raw($"[Updater] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.Blue);
        }
        
        public static void Silent(string message)
        {
            History.Add(new(DateTime.Now, LogLevel.Debug, message));
            if (Plugin.Instance.Config.ShowSilentLogs)
                Logger.Raw($"[Silent] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.White);
        }

        public static void System(string message) => History.Add(new(DateTime.Now, LogLevel.Info, message));

        public static void Security(string message)
        {
            History.Add(new(DateTime.Now, LogLevel.Info, message));
            Logger.Raw($"[Security] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.DarkMagenta);
        }
    }
}
