using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics
{
    public struct Portal : IComponentData
    {
        public Entity Companion;

        public Portal(Entity companion) => Companion = companion;
    }

    public sealed class PortalComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject Companion;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var companionEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(Companion, dstManager.World);
            Destroy(Companion);
            dstManager.AddComponentData(entity, new Portal(companionEntity));
            dstManager.AddComponentData(companionEntity, new Portal(entity));
        }
    }
}
