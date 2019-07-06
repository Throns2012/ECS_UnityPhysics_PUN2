using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics
{
    public sealed class DisabledComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Disabled());
        }
    }
}