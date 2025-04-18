﻿using Exiled.API.Enums;
using UncomplicatedCustomItems.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class DynamicSpawn : IDynamicSpawn
    {
        public RoomType Room { get; set; } = RoomType.Lcz330;
        public int Chance { get; set; } = 30;
        public Vector3 Coords { get; set; } = new Vector3(0, 0, 0);
    }
}
