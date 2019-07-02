using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics
{
    public struct DestroyableComponentData : IComponentData
    {
        public bool ShouldDestroy;
    }

    public sealed class DestroyableComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new DestroyableComponentData());
        }
    }
}
