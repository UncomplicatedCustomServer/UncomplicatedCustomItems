using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomItems.API.Attributes;
using UncomplicatedCustomItems.API.Features;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Enums
{
    public enum CommonTypes
    {
        [CommonTypes(typeof(Vector2))]
        Vector2,

        [CommonTypes(typeof(Vector3))]
        Vector3,

        [CommonTypes(typeof(Vector4))]
        Vector4,

        [CommonTypes(typeof(Quaternion))]
        Quaternion,

        [CommonTypes(typeof(Transform))]
        Transform,

        [CommonTypes(typeof(GameObject))]
        GameObject,

        [CommonTypes(typeof(Timing))]
        Timing,

        [CommonTypes(typeof(Player))]
        Player,

        [CommonTypes(typeof(Room))]
        Room,

        [CommonTypes(typeof(Item))]
        Item,

        [CommonTypes(typeof(SummonedCustomItem))]
        SummonedCustomItem,

        [CommonTypes(typeof(CustomItem))]
        CustomItem,

        [CommonTypes(typeof(CustomAction))]
        CustomAction,

        [CommonTypes(typeof(Announcer))]
        Cassie,

        [CommonTypes(typeof(Announcer))]
        Announcer,

        [CommonTypes(typeof(Elevator))]
        Elevator,

        [CommonTypes(typeof(Pickup))]
        Pickup,

        [CommonTypes(typeof(Map))]
        Map,

        [CommonTypes(typeof(Projectile))]
        Projectile,

        [CommonTypes(typeof(Round))]
        Round,

        [CommonTypes(typeof(Server))]
        Server,

        [CommonTypes(typeof(Physics))]
        Physics,

        [CommonTypes(typeof(Ragdoll))]
        Ragdoll,

        [CommonTypes(typeof(RespawnWave))]
        RespawnWave,

        [CommonTypes(typeof(LabApi.Features.Wrappers.Scp914))]
        Scp914,

        [CommonTypes(typeof(Tesla))]
        Tesla,

        [CommonTypes(typeof(ThrowableItem))]
        ThrowableItem,

        [CommonTypes(typeof(Workstation))]
        Workstation,

        [CommonTypes(typeof(Door))]
        Door,

        [CommonTypes(typeof(BreakableDoor))]
        BreakableDoor,

        [CommonTypes(typeof(Gate))]
        Gate,

        [CommonTypes(typeof(ElevatorDoor))]
        ElevatorDoor,

        [CommonTypes(typeof(BulkheadDoor))]
        BulkheadDoor,
        
        [CommonTypes(typeof(DummyDoor))]
        DummyDoor,

        [CommonTypes(typeof(DoorCrusher))]
        DoorCrusher,
    }
}