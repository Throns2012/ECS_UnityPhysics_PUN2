using System;
using System.Collections.Generic;
using Assets.MyFolder.Scripts.Utility;
using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics
{
    public sealed class PrefabComponent
        : MonoBehaviour,
            IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Prefab());
            if (FindComponentOfInterfaceOrClassHelper.FindComponentOfInterfaceOrClass(out IPrefabStorage storage))
                storage.Add(entity);
            else throw new KeyNotFoundException();
        }
    }
}