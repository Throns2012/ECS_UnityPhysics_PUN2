using Assets.MyFolder.Scripts.Basics;
using Photon.Pun;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts
{
    public sealed class FindPlayerEntityHelper : ComponentSystem
    {
        private EntityQuery _query;
        public PhotonView View;

        protected override void OnCreate()
        {
            var cs = new NativeArray<ComponentType>(5, Allocator.Temp)
            {
                [0] = ComponentType.ReadOnly<Translation>(),
                [1] = ComponentType.ReadOnly<Rotation>(),
                [2] = ComponentType.ReadOnly<PhysicsVelocity>(),
                [3] = ComponentType.ReadOnly<PlayerMachineTag>(),
                [4] = ComponentType.ReadOnly<TeamTag>(),
            };
            RequireForUpdate(_query = GetEntityQuery(cs));
            cs.Dispose();
            Enabled = false;
        }

        public Entity Find()
        {
            if (View == null)
            {
                return Entity.Null;
            }
            var actor = View.OwnerActorNr;
            using (var array = _query.ToEntityArray(Allocator.TempJob))
            {
                foreach (var entity in array)
                {
                    if (EntityManager.GetComponentData<TeamTag>(entity).Id == actor)
                        return entity;
                }
            }
            return Entity.Null;
        }

        protected override void OnUpdate() { }
    }
}