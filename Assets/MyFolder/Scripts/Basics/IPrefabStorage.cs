using System.Collections.Generic;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public interface IPrefabStorage
    {
        void Add(Entity entity);
        IEnumerable<Entity> Prefabs { get; }

        bool FindPrefab<T>(EntityManager manager, out Entity prefab) where T : struct, IComponentData;
        bool FindPrefab<T0, T1>(EntityManager manager, out Entity prefab)
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData;
    }
}
