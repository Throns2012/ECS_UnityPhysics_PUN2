using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics
{
    public sealed class Team : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Id;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!enabled) return;
            dstManager.AddComponentData(entity, new TeamTag()
            {
                Id = Id
            });
        }
    }

    public struct TeamTag : IComponentData
    {
        public int Id;
    }
}
