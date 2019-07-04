using Assets.MyFolder.Scripts.Basics.CameraManipulation;
using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics
{
    public sealed class ManualCameraComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        public ManualCameraKeyMapScriptableObject KeyMap;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!enabled) return;
            dstManager.AddComponentData(entity, (ManualCameraControlSingleton)KeyMap);
        }

        void OnDisable() { }
    }
}
