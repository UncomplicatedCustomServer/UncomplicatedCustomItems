using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Logger = LabApi.Features.Console.Logger;

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
                Logger.Raw($"[DEBUG] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {FormatLogMessage(message)}", ConsoleColor.Green);
        }

        public static void Info(string message)
        {
            History.Add(new(DateTime.Now, LogLevel.Info, message));
            Logger.Info(FormatLogMessage(message));
        }

        public static void Warn(string message, string error = "CS0000")
        {
            History.Add(new(DateTime.Now, LogLevel.Warn, message));
            Logger.Warn(FormatLogMessage(message));
        }

        public static void Error(string message, string error = "CS0000")
        {
            History.Add(new(DateTime.Now, LogLevel.Error, message));
            Logger.Error(FormatLogMessage(message));
        }

        public static void Raw(string message, ConsoleColor color, LogLevel logLevel, string category)
        {
            History.Add(new(DateTime.Now, logLevel, message));
            Logger.Raw($"[{category}] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {FormatLogMessage(message)}", color);
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

        internal static string FormatLogMessage(string message)
        {
            StackTrace stackTrace = new(true);
            StackFrame? frame = stackTrace.GetFrame(2);

            string source = "Unknown";

            if (frame != null)
            {
                MethodBase? method = frame.GetMethod();
                if (method?.DeclaringType != null)
                {
                    source = GetReadableMethodName(method);
                }
            }

            message = Regex.Replace(message, @"_Patch\d+", "");
            return $"[{source}] {message}";
        }

        private static string GetReadableMethodName(MethodBase method)
        {
            Type declaringType = method.DeclaringType!;
            string typeName = declaringType.FullName ?? declaringType.Name;
            string methodName = method.Name;

            bool isCompilerGenerated = method.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0
                || declaringType.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0
                || typeName.Contains("<>")
                || typeName.Contains("__")
                || methodName.StartsWith("<");

            if (!isCompilerGenerated)
            {
                return FormatTypeAndMethod(typeName, methodName, method.IsStatic);
            }

            if (typeName.Contains("+<>c") && methodName.StartsWith("<"))
            {
                int endIdx = methodName.IndexOf(">b__");
                if (endIdx > 1)
                {
                    return FormatTypeAndMethod(typeName.Substring(0, typeName.IndexOf("+<>c")), methodName.Substring(1, endIdx - 1), true);
                }
            }

            if (typeName.Contains("+<") && typeName.Contains(">d__"))
            {
                int start = typeName.IndexOf("+<") + 2;
                int end = typeName.IndexOf(">d__", start);
                if (start > 1 && end > start)
                {
                    return FormatTypeAndMethod(typeName.Substring(0, typeName.IndexOf("+<")), typeName.Substring(start, end - start), false);
                }
            }

            if (methodName.StartsWith("<") && methodName.Contains(">g__"))
            {
                int endIdx = methodName.IndexOf(">g__");
                if (endIdx > 1)
                {
                    string outerType = typeName;
                    if (outerType.Contains("+<"))
                        outerType = outerType.Substring(0, outerType.IndexOf("+<"));

                    return FormatTypeAndMethod(outerType, methodName.Substring(1, endIdx - 1), method.IsStatic);
                }
            }

            return FormatTypeAndMethod(typeName, methodName, method.IsStatic);
        }

        private static string FormatTypeAndMethod(string typeName, string methodName, bool isStatic)
        {
            string separator = isStatic ? "." : "::";
            return $"{typeName}{separator}{methodName}()";
        }
    }
}
