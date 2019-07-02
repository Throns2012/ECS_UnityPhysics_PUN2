using Assets.MyFolder.Scripts.Basics;
using Unity.Collections;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public sealed class DestroySystem : ComponentSystem
    {
        private EntityQuery _query;
        protected override void OnCreate()
        {
            _query = GetEntityQuery(ComponentType.ReadOnly<DestroyableComponentData>());
        }

        protected override void OnUpdate()
        {
            var typeEntity = GetArchetypeChunkEntityType();
            var type = GetArchetypeChunkComponentType<DestroyableComponentData>();
            NativeList<Entity> toDestroy = new NativeList<Entity>(256, Allocator.Temp);
            try
            {
                using (var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob))
                {
                    foreach (var chunk in chunks)
                    {
                        var entities = chunk.GetNativeArray(typeEntity);
                        var destroyables = chunk.GetNativeArray(type);
                        for (var i = destroyables.Length - 1; i >= 0; i--)
                        {
                            if (!destroyables[i].ShouldDestroy) continue;
                            toDestroy.Add(entities[i]);
                        }
                    }
                }
            }
            finally
            {
                EntityManager.DestroyEntity(toDestroy);
                toDestroy.Dispose();
            }
        }
    }
}
