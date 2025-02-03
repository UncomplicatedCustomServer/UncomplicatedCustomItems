using Discord;
using Exiled.API.Features;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using UncomplicatedCustomItems;
using UncomplicatedCustomItems.Interfaces;
using UncomplicatedCustomItems.Elements;
using YamlDotNet.Core.Tokens;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API;

namespace UncomplicatedCustomItems.Manager
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

        public static void Silent(string message) => History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "Silent", message));

        public static void System(string message) => History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "System", message));

        internal static HttpStatusCode SendReport(out HttpContent content)
        {
            content = null;

            if (MessageSent)
                return HttpStatusCode.Forbidden;

            if (History.Count < 1)
                return HttpStatusCode.Forbidden;

            string stringContent = string.Empty;

            foreach (LogEntry Element in History)
                stringContent += $"{Element}\n";

            stringContent += "\n======== BEGIN CUSTOM ITEMS ========\n";

            foreach (ICustomItem Item in Utilities.Items.Values)
                stringContent += $"{Loader.Serializer.Serialize(Item)}\n\n---\n\n";

            HttpStatusCode Response = Plugin.HttpManager.ShareLogs(stringContent, out content);

            if (Response is HttpStatusCode.OK)
                MessageSent = true;

            return Response;
        }
    }
}