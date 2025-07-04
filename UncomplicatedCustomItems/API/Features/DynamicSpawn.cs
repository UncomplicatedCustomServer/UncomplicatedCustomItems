﻿using Exiled.API.Enums;
using UncomplicatedCustomItems.API.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class DynamicSpawn : IDynamicSpawn
    {
        public virtual RoomType Room { get; set; } = RoomType.Lcz330;
        public virtual int Chance { get; set; } = 30;
        public virtual Vector3 Coords { get; set; } = new Vector3(0, 0, 0);
    }
}
