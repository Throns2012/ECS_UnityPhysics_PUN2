using ExitGames.Client.Photon;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics
{
    public sealed unsafe class SyncInfo
    {
        public float3 Position;
        public quaternion Rotation;
        public PhysicsVelocity Velocity;

        private static readonly byte[] tmp;

        private static readonly SyncInfo ReturnValue;

        static SyncInfo()
        {
            tmp = new byte[sizeof(float3) + sizeof(PhysicsVelocity)];
            ReturnValue = new SyncInfo();
            for (var code = 0; code <= byte.MaxValue; code++)
            {
                if (!PhotonPeer.RegisterType(typeof(SyncInfo), (byte)code, Serialize, Deserialize)) continue;
                break;
            }
        }

        private static short Serialize(StreamBuffer outStream, object customObject)
        {
            if (!(customObject is SyncInfo syncInfo)) return 0;
            var ptr = (float3*)UnsafeUtility.PinGCArrayAndGetDataAddress(tmp, out var handle);
            *ptr++ = syncInfo.Position;
            var quaternionPtr = (quaternion*)ptr;
            *quaternionPtr++ = syncInfo.Rotation;
            *(PhysicsVelocity*)quaternionPtr = syncInfo.Velocity;
            UnsafeUtility.ReleaseGCObject(handle);
            return (short)(sizeof(float3) + sizeof(quaternion) + sizeof(PhysicsVelocity));
        }

        private static object Deserialize(StreamBuffer outStream, short length)
        {
            if (length == 0) return null;
            outStream.Read(tmp, 0, length);
            var ptr = (float3*)UnsafeUtility.PinGCArrayAndGetDataAddress(tmp, out var handle);
            ReturnValue.Position = *ptr++;
            var quaternionPtr = (quaternion*)ptr;
            ReturnValue.Rotation = *quaternionPtr++;
            ReturnValue.Velocity = *(PhysicsVelocity*)quaternionPtr;
            UnsafeUtility.ReleaseGCObject(handle);
            return ReturnValue;
        }
    }
}
