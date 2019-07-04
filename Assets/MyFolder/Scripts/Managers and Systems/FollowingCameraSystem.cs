using Assets.MyFolder.Scripts.Basics;
using Assets.MyFolder.Scripts.Basics.CameraManipulation;
using Photon.Pun;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed class FollowingCameraSystem : ComponentSystem
    {
        public Camera MainCamera
        {
            get => _mainCamera;
            set
            {
                _mainCamera = value;
                if (_mainCamera == null) return;
                _mainCameraTransform = _mainCamera.transform;
            }
        }

        public float MoveConstant = 20f, RotateConstant = 45f;
        private Transform _mainCameraTransform;
        private Camera _mainCamera;

        private Entity _cameraTargetEntity;
        private EntityQuery _query;
        public PhotonView View;
        private float _distance;

        protected override void OnCreate()
        {
            RequireForUpdate(_query = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PlayerMachineTag>(), ComponentType.ReadOnly<TeamTag>()));
            RequireSingletonForUpdate<FollowingCameraControlSingleton>();
        }

        protected override void OnStartRunning()
        {
            _cameraTargetEntity = Entity.Null;
            using (var entities = _query.ToEntityArray(Allocator.TempJob))
            {
                foreach (var entity in entities)
                {
                    if (EntityManager.GetComponentData<TeamTag>(entity).Id != View.OwnerActorNr) continue;
                    _cameraTargetEntity = entity;
                }
            }
            if (_cameraTargetEntity == Entity.Null) return;
            _distance = math.distance(EntityManager.GetComponentData<Translation>(_cameraTargetEntity).Value, _mainCameraTransform.position);
        }

        protected override void OnUpdate()
        {
            var keyMap = GetSingleton<FollowingCameraControlSingleton>();
            if (_cameraTargetEntity == Entity.Null) return;
            var worldPosition = EntityManager.GetComponentData<Translation>(_cameraTargetEntity).Value;
            var pos = (float3)_mainCameraTransform.position;
            var normal = math.normalize(pos - worldPosition);
            var theta = math.atan2(normal.x, normal.z);
            var phi = math.asin(normal.y);

            var delta = Time.deltaTime;

            if (Input.GetKey(keyMap.MoveZAxisPlus))
                _distance -= delta * MoveConstant;
            if (Input.GetKey(keyMap.MoveZAxisMinus))
                _distance += delta * MoveConstant;

            if (Input.GetKey(keyMap.MoveXAxisPlus))
                theta -= delta;
            if (Input.GetKey(keyMap.MoveXAxisMinus))
                theta += delta;

            if (Input.GetKey(keyMap.MoveYAxisPlus))
                phi += delta;
            if (Input.GetKey(keyMap.MoveYAxisMinus))
                phi -= delta;


            var x = math.cos(phi) * math.sin(theta);
            var y = math.sin(phi);
            var z = math.cos(phi) * math.cos(theta);

            _mainCameraTransform.position = (Vector3)worldPosition + new Vector3(x, y, z) * _distance;
            _mainCameraTransform.LookAt(worldPosition);
        }
    }
}
