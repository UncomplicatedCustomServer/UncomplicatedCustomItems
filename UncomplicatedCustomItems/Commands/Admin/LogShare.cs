using CommandSystem;
using System;
using System.Net;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Collections.Generic;
using LabApi.Features.Console;
using System.Text.Json;
using UncomplicatedCustomItems.API.Features.Networking;
using System.Text.Json.Serialization;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class LogShare : ParentCommand
    {
        public class Response
        {
            [JsonPropertyName("response")]
            public string Message { get; set; } = string.Empty;

            [JsonPropertyName("code")]
            public string Id { get; set; } = string.Empty;
        }

        public LogShare() => LoadGeneratedCommands();

        public override string Command { get; } = "ucilogs";

        public override string[] Aliases { get; } = [];

        public override string Description { get; } = "Share the UCI Debug logs with the developers.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.LogName is not "SERVER CONSOLE")
            {
                response = "Sorry but this command is reserved to the game console!";
                return false;
            }

            response = "Uploading logs to the developers... This may take a moment.";

            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            ShareLogsRequest request = new();
            Logger.Info("[ShareTheLog] Starting log upload process...");
            request.SendRequest((request) =>
            {
                Response? response = JsonSerializer.Deserialize<Response>(request.downloadHandler.text);
                if (response != null)
                {
                    Logger.Info("[ShareTheLog] Successfully shared the UCI logs with the developers!");
                    Logger.Info($"[ShareTheLog] Send this ID to the developers: {response.Id}");
                    Logger.Info($"[ShareTheLog] Operation completed in {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms");
                }
            });
            return true;
        }
    }
}