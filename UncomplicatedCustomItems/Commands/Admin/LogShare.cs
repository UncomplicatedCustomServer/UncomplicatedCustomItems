using CommandSystem;
using System;
using System.Net;
using UncomplicatedCustomItems.API.Features.Helper;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UncomplicatedCustomItems.Extensions;
using Exiled.API.Features;

namespace UncomplicatedCustomItems.Commands.Admin
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class LogShare : ParentCommand
    {
        public LogShare() => LoadGeneratedCommands();

        public override string Command { get; } = "ucilogs";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Share the UCI Debug logs with the developers";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.LogName is not "SERVER CONSOLE")
            {
                response = "Sorry but this command is reserved to the game console!";
                return false;
            }

            long Start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            response = $"Loading the JSON content to share with the developers...";

            Task.Run(() =>
            {
                HttpStatusCode Response = LogManager.SendReport(out HttpContent Content);
                try
                {
                    if (Response is HttpStatusCode.OK)
                    {
                        Dictionary<string, string> Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(Plugin.HttpManager.RetriveString(Content));
                        Log.Info($"[ShareTheLog] Successfully shared the UCI logs with the developers!\nSend this Id to the developers: {Data["id"]}\n\nTook {DateTimeOffset.Now.ToUnixTimeMilliseconds() - Start}ms");
                    } 
                    else
                        Log.Info($"Failed to share the UCI logs with the developers: Server says: {Response}");
                }
                catch (Exception e) 
                { 
                    Log.Error(e.ToString()); 
                }
            });
            

            return true;
        }
    }
}