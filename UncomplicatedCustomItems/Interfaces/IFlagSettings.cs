using Exiled.API.Enums;
using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoiceChat.Networking;

namespace UncomplicatedCustomItems.Interfaces
{
    #nullable enable
    public interface IFlagSettings
    {
        public abstract string GlowColor { get; set; }
        
        public abstract bool GlowInHand { get; set; }

        public abstract float LifeStealAmount { get; set; }

        public abstract float LifeStealPercentage { get; set; }
    }
}