using CommandSystem;
using System;
using System.Net;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Collections.Generic;
using LabApi.Features.Console;
using System.Text.Json;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class LogShare : ParentCommand
    {
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
            Logger.Info("[ShareTheLog] Starting log upload process...");
            LogManager.SendReport((status, responseContent, readableSize) =>
            {
                long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - start;
                if (status == HttpStatusCode.OK)
                {
                    try
                    {
                        Dictionary<string, JsonElement> data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent);

                        Logger.Info($"[ShareTheLog] Data size being sent: {readableSize}");
                        Logger.Info("[ShareTheLog] Successfully shared the UCI logs with the developers!");

                        if (data.TryGetValue("id", out JsonElement idElement))
                        {
                            string id = idElement.GetString();
                            Logger.Info($"[ShareTheLog] Send this ID to the developers: {id}");
                        }
                        else
                            Logger.Warn("[ShareTheLog] Server response did not contain an ID.");

                        Logger.Info($"[ShareTheLog] Operation completed in {elapsed}ms");
                    }
                    catch (JsonException jsonEx)
                    {
                        Logger.Error($"[ShareTheLog] Failed to parse server response: {jsonEx.Message}");
                    }
                }
                else
                {
                    Logger.Error($"[ShareTheLog] Failed to share the UCI logs with the developers. Server response: {status}");

                    if (!string.IsNullOrEmpty(responseContent))
                        Logger.Debug($"[ShareTheLog] Response body: {responseContent}");
                }
            });

            return true;
        }
    }
}