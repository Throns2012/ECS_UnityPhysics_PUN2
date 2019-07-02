using Assets.MyFolder.Scripts.Basics;
using Photon.Pun;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    [AlwaysUpdateSystem]
    public sealed class FrameIntervalSyncStarterSystem : ComponentSystem
    {
        public ISynchronizer Synchronizer;
        public PhotonView View;
        private EntityQuery _query;
        private int _frameCount = 15;
        private int _frameInterval = -1;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(ComponentType.ReadWrite<DestroyableComponentData>(), ComponentType.ReadOnly<TeamTag>(), ComponentType.ReadOnly<PhysicsVelocity>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Rotation>());
        }

        protected override unsafe void OnUpdate()
        {
            if (Synchronizer is null || View is null) return;
            if (++_frameInterval != _frameCount) return;
            _frameInterval = -1;
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var typePhysicsVelocity = GetArchetypeChunkComponentType<PhysicsVelocity>();
            var typeTranslation = GetArchetypeChunkComponentType<Translation>();
            var typeRotation = GetArchetypeChunkComponentType<Rotation>();
            var typeTeamTag = GetArchetypeChunkComponentType<TeamTag>(true);
            foreach (var chunk in chunks)
            {
                var velocities = chunk.GetNativeArray(typePhysicsVelocity);
                var translations = chunk.GetNativeArray(typeTranslation);
                var rotations = chunk.GetNativeArray(typeRotation);
                var teamTags = chunk.GetNativeArray(typeTeamTag);
                for (var i = 0; i < teamTags.Length; i++)
                {
                    if (teamTags[i].Id != View.OwnerActorNr) continue;
                    Synchronizer.Sync(
                        ((Translation*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(translations))[i],
                        ((Rotation*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(rotations))[i],
                        ((PhysicsVelocity*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(velocities))[i]);
                    goto RETURN;
                }
            }
        RETURN:
            chunks.Dispose();
        }
    }
}
