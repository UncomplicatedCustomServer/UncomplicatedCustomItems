using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LabApi.Features.Wrappers;
using MapGeneration;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.CustomItemAPI;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Components
{
    public enum Segment
    {
        Base = 0,
        TotalItems,
        ZoneInformation,

        All = Base | TotalItems | ZoneInformation,
    }
    
    public class DebugUI : MonoBehaviour
    {
        private Player? Player;
        private StringBuilder? Builder;
        public readonly List<Segment> ActiveSegments = [];

        public void Init(Player player)
        {
            Player = player;
            Builder = new();
        }

        private void Update()
        {
            if (ActiveSegments.Count <= 0 || Builder == null || Player == null)
                return;

            Builder.Clear();

            HandleBaseSegment();
            HandleTotalItemsSegment();
            HandleZoneInformationSegment();

            Player.SendHint(Builder.ToString(), 0.5f);
        }

        public void RemoveSegment(Segment segment)
        {
            ActiveSegments.Remove(segment);
        }

        public void AddSegment(Segment segment)
        {
            if (!ActiveSegments.Contains(segment))
                ActiveSegments.Add(segment);
        }

        private void HandleBaseSegment()
        {
            if (!ActiveSegments.Contains(Segment.Base) || Builder == null || Player == null)
                return;

            Builder.AppendLine($"UCI Debug Menu {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Builder.AppendLine($"");
            Builder.AppendLine($"Room Name: {Player.CachedRoom?.Name ?? RoomName.Unnamed}");
            Builder.AppendLine($"Local Position: {Player.CachedRoom?.LocalPosition(Player.Position)}");
            Builder.AppendLine($"World Position: {Player.CachedRoom?.Position}");
        }

        private void HandleTotalItemsSegment()
        {
            if (!ActiveSegments.Contains(Segment.TotalItems) || Builder == null || Player == null)
                return;

            Builder.AppendLine($"Total Custom Items: {CustomItem.List.Count()}");
            Builder.AppendLine($"Total Summoned Custom Items: {SummonedCustomItem.List.Count()}");
            Builder.AppendLine($"Total API Custom Items: {APICustomItem.List.Count()}");
            Builder.AppendLine($"Total Summoned API Custom Items: {SummonedAPICustomItem.List.Count()}");
        }

        private void HandleZoneInformationSegment()
        {
            if (!ActiveSegments.Contains(Segment.ZoneInformation) || Builder == null || Player == null)
                return;

            Builder.AppendLine($"Total Custom Items on Surface Zone: {SummonedCustomItem.List.Where(s => s.IsPickup && s.Pickup!.Room!.Zone == FacilityZone.Surface).Count() + SummonedAPICustomItem.List.Where(s => s.IsPickup && s.Pickup!.Room!.Zone == FacilityZone.Surface).Count()}");
            Builder.AppendLine($"Total Custom Items in Entrance Zone: {SummonedCustomItem.List.Where(s => s.IsPickup && s.Pickup!.Room!.Zone == FacilityZone.Entrance).Count() + SummonedAPICustomItem.List.Where(s => s.IsPickup && s.Pickup!.Room!.Zone == FacilityZone.Entrance).Count()}");
            Builder.AppendLine($"Total Custom Items in Heavy Containment Zone: {SummonedCustomItem.List.Where(s => s.IsPickup && s.Pickup!.Room!.Zone == FacilityZone.HeavyContainment).Count() + SummonedAPICustomItem.List.Where(s => s.IsPickup && s.Pickup!.Room!.Zone == FacilityZone.HeavyContainment).Count()}");
            Builder.AppendLine($"Total Custom Items in Light Containment Zone: {SummonedCustomItem.List.Where(s => s.IsPickup && s.Pickup!.Room!.Zone == FacilityZone.LightContainment).Count() + SummonedAPICustomItem.List.Where(s => s.IsPickup && s.Pickup!.Room!.Zone == FacilityZone.LightContainment).Count()}");
        }
    }
}