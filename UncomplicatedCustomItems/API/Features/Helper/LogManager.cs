﻿using Discord;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using UncomplicatedCustomItems.API.Interfaces;
using Exiled.API.Features;

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
            Log.Debug(message);
        }

        public static void Info(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Info.ToString(), message));
            Log.Info(message);
        }

        public static void Warn(string message, string error = "CS0000")
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString(), message, error));
            Log.Warn(message);
        }

        public static void Error(string message, string error = "CS0000")
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString(), message, error));
            Log.Error(message);
        }
        public static void Raw(string message, ConsoleColor color)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString(), message));
            Log.SendRaw($"[Updater] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", color);
        }

        public static void Silent(string message) => History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "SILENT", message));

        public static void System(string message) => History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "SYSTEM", message));

        public static HttpStatusCode SendReport(out HttpContent content)
        {
            content = null;

            if (MessageSent)
                {
                    return HttpStatusCode.Forbidden;
                }

            if (History.Count < 1)
                {
                    return HttpStatusCode.Forbidden;
                }


            string Content = string.Empty;

            foreach (LogEntry Element in History)
                Content += $"{Element}\n";
            
            // Now let's add the separator
            Content += "\n======== BEGIN CUSTOM ITEMS ========\n";

            foreach (ICustomItem Item in CustomItem.List)
            {
                Content += $"{Loader.Serializer.Serialize(Item)}\n\n---\n\n";
            }

            HttpStatusCode Response = Plugin.HttpManager.ShareLogs(Content, out content);

            if (Response is HttpStatusCode.OK)
            {
                MessageSent = true;
            }

            return Response;
        }
    }
}
