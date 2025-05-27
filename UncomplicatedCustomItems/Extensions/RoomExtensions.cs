using LabApi.Features.Wrappers;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UncomplicatedCustomItems.Extensions
{
    internal static class RoomExtensions
    {
        /// <summary>
        /// Returns the local space position, based on a world space position.
        /// </summary>
        /// <param name="room">The room instance this method extends.</param>
        /// <param name="position">World position.</param>
        /// <returns>Local position, based on the room.</returns>
        public static Vector3 LocalPosition(this Room room, Vector3 position) => room.Transform.InverseTransformPoint(position);

        /// <summary>
        /// Returns the World position, based on a local space position.
        /// </summary>
        /// <param name="room">The room instance this method extends.</param>
        /// <param name="offset">Local position.</param>
        /// <returns>World position, based on the room.</returns>
        public static Vector3 WorldPosition(this Room room, Vector3 offset) => room.Transform.TransformPoint(offset);

        /// <summary>
        /// Gets a room by its GameObject name.
        /// </summary>
        /// <param name="rooms">The collection of rooms to search through.</param>
        /// <param name="gameObjectName">The name of the GameObject to search for.</param>
        /// <returns>The room with the matching GameObject name, or null if not found.</returns>
        public static Room GetByGameObjectName(this IReadOnlyCollection<Room> rooms, string gameObjectName)
        {
            return rooms.FirstOrDefault(room => room.GameObject.name == gameObjectName);
        }
    }
}