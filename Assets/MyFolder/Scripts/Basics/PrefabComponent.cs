using System;
using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics
{
    public sealed class PrefabComponent
        : MonoBehaviour,
            IConvertGameObjectToEntity
    {
        public
#if UNITY_2019_3_OR_NEWER
            IPrefabStorage
#else
            MonoBehaviour
#endif
            Storage;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Prefab());
#if UNITY_2019_3_OR_NEWER
            Storage.Add(entity);
#else
            var storage = ((object)Storage) as IPrefabStorage;
            if (storage is null) throw new InvalidCastException();
            storage.Add(entity);
#endif
        }
    }
}