using MapGeneration;
using UncomplicatedCustomItems.API.Enums;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features
{
    public class LockerSpawn
    {
        public bool Enable { get; set; }
        public LockerType LockerType { get; set; }
        public string Room { get; set; } = "HczWarhead";
        public FacilityZone Zone { get; set; } = FacilityZone.HeavyContainment;
        public string Chamber { get; set; } = "";
        public Vector3 Offset { get; set; } = Vector3.zero;
    }
}