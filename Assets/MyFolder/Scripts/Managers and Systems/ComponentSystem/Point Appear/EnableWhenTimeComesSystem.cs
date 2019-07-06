using System;
using Assets.MyFolder.Scripts.Basics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed class EnableWhenTimeComesSystem : ComponentSystem
    {
        public Entity CurrentPointPrefab;
        private EntityQuery _query;
        protected override void OnCreate()
        {
            var c2 = new NativeArray<ComponentType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
            {
                [0] = ComponentType.ReadOnly<DateTimeTicksToProcess>(),
                [1] = ComponentType.ReadOnly<Disabled>(),
                [2] = ComponentType.ReadOnly<Translation>(),
                [3] = ComponentType.ReadOnly<Point>(),
            };
            _query = GetEntityQuery(c2);
            c2.Dispose();
        }

        protected override void OnUpdate()
        {
            var typeTicks = GetArchetypeChunkComponentType<DateTimeTicksToProcess>(true);
            var typeTranslation = GetArchetypeChunkComponentType<Translation>(true);
            var typePoint = GetArchetypeChunkComponentType<Point>(true);
            var typeEntities = GetArchetypeChunkEntityType();
            var current = DateTime.Now.Ticks;
            using (var entityList = new NativeList<Entity>(Allocator.Temp))
            using (var translationList = new NativeList<Translation>(Allocator.Temp))
            using (var pointList = new NativeList<Point>(Allocator.Temp))
            {
                using (var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob))
                {
                    for (var chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
                    {
                        var chunk = chunks[chunkIndex];
                        var ticks = chunk.GetNativeArray(typeTicks);
                        var translations = chunk.GetNativeArray(typeTranslation);
                        var points = chunk.GetNativeArray(typePoint);
                        var entities = chunk.GetNativeArray(typeEntities);
                        for (var index = 0; index < ticks.Length; index++)
                        {
                            if (current < ticks[index].Value) continue;
                            entityList.Add(entities[index]);
                            translationList.Add(translations[index]);
                            pointList.Add(points[index]);
                        }
                    }
                }
                var entityArray = (NativeArray<Entity>)entityList;
                var translationArray = (NativeArray<Translation>)translationList;
                var pointArray = (NativeArray<Point>)pointList;
                EntityManager.DestroyEntity(entityArray);
                EntityManager.Instantiate(CurrentPointPrefab, entityArray);
                for (var i = 0; i < entityArray.Length; i++)
                {
                    EntityManager.SetComponentData(entityArray[i], translationArray[i]);
                    EntityManager.SetComponentData(entityArray[i], pointArray[i]);
                }
            }
        }
    }
}
