using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs.Server;
using LabApi.Events.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems.Events
{
    internal class ServerHandler
    {
        public static void Register()
        {
            Exiled.Events.Handlers.Server.EndingRound += OnRoundEnd;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Server.EndingRound -= OnRoundEnd;
        }

        public static void OnRoundEnd(EndingRoundEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            PlayerHandler._damageTimes.Clear();
            PlayerHandler.Appearance.Clear();
            PlayerHandler.Capybara.Clear();
        }

    }
}
