using CommandSystem;
using System;
using System.Net;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using LabApi.Features.Console;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class LogShare : ParentCommand
    {
        public LogShare() => LoadGeneratedCommands();

        public override string Command { get; } = "ucilogs";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Share the UCI Debug logs with the developers.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.LogName is not "SERVER CONSOLE")
            {
                response = "Sorry but this command is reserved to the game console!";
                return false;
            }

            response = "Loading the JSON content to share with the developers... This may take a moment.";

            _ = Task.Run(async () =>
            {
                long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                
                try
                {
                    Logger.Info("[ShareTheLog] Starting log upload process...");
                    
                    var result = await LogManager.SendReportAsync().ConfigureAwait(false);
                    
                    if (result.statusCode == HttpStatusCode.OK)
                    {
                        string responseContent = Plugin.HttpManager.RetriveString(result.content);
                        Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                        
                        long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - start;
                        
                        Logger.Info($"[ShareTheLog] Data size being sent: {result.readableSize}");
                        Logger.Info($"[ShareTheLog] Successfully shared the UCI logs with the developers!");
                        Logger.Info($"[ShareTheLog] Send this ID to the developers: {data["id"]}");
                        Logger.Info($"[ShareTheLog] Operation completed in {elapsed}ms");
                    }
                    else
                    {
                        Logger.Error($"[ShareTheLog] Failed to share the UCI logs with the developers. Server response: {result.statusCode}");
                    }
                }
                catch (JsonException jsonEx)
                {
                    Logger.Error($"[ShareTheLog] Failed to parse server response: {jsonEx.Message}");
                }
                catch (HttpRequestException httpEx)
                {
                    Logger.Error($"[ShareTheLog] Network error during log upload: {httpEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[ShareTheLog] Unexpected error during log upload: {ex}");
                }
            });

            return true;
        }
    }
}