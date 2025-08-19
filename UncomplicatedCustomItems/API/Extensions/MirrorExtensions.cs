
using System;
using LabApi.Features.Wrappers;
using Mirror;

namespace UncomplicatedCustomItems.API.Extensions
{
    internal static class MirrorExtensions
    {
        // Taken from Exiled
        public static void SendFakeSyncObject(Player target, NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customAction)
        {
            if (target.GameObject is not null)
            {
                NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get();
                NetworkWriterPooled networkWriterPooled2 = NetworkWriterPool.Get();
                MakeCustomSyncWriter(behaviorOwner, targetType, customAction, null, networkWriterPooled, networkWriterPooled2);
                target.ReferenceHub.networkIdentity.connectionToClient.Send(new EntityStateMessage
                {
                    netId = behaviorOwner.netId,
                    payload = networkWriterPooled.ToArraySegment()
                });
                NetworkWriterPool.Return(networkWriterPooled);
                NetworkWriterPool.Return(networkWriterPooled2);
            }
        }

        private static void MakeCustomSyncWriter(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncObject, Action<NetworkWriter> customSyncVar, NetworkWriter owner, NetworkWriter observer)
        {
            ulong value = 0uL;
            NetworkBehaviour networkBehaviour = null;
            for (int i = 0; i < behaviorOwner.NetworkBehaviours.Length; i++)
            {
                if (behaviorOwner.NetworkBehaviours[i].GetType() == targetType)
                {
                    networkBehaviour = behaviorOwner.NetworkBehaviours[i];
                    value = (ulong)(1L << (i & 0x1F));
                    break;
                }
            }

            Compression.CompressVarUInt(owner, value);
            int position = owner.Position;
            owner.WriteByte(0);
            int position2 = owner.Position;
            if (customSyncObject != null)
            {
                customSyncObject(owner);
            }
            else
            {
                networkBehaviour.SerializeObjectsDelta(owner);
            }

            customSyncVar?.Invoke(owner);
            int position3 = owner.Position;
            owner.Position = position;
            owner.WriteByte((byte)((position3 - position2) & 0xFF));
            owner.Position = position3;
            if (networkBehaviour.syncMode != SyncMode.Observers)
            {
                observer.WriteBytes(owner.ToArraySegment().Array, position, owner.Position - position);
            }
        }
    }
}