using Assets.MyFolder.Scripts.Basics;
using Assets.MyFolder.Scripts.Utility;
using Photon.Pun;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed class ControlPlayerMachineMoreComplexSystem : ComponentSystem
    {
        public Camera MainCamera
        {
            get => _camera;
            set
            {
                _camera = value;
                _cameraTransform = value.transform;
            }
        }

        private Camera _camera;
        private Transform _cameraTransform;
        public IComplexMoveNotifier Notifier;
        private EntityQuery _query;
        public Entity PlayerEntity;
        public PhotonView View;

        protected override void OnCreate()
        {
            FindComponentOfInterfaceOrClassHelper.FindComponentOfInterfaceOrClass(out _camera);
            _cameraTransform = _camera.transform;
            FindComponentOfInterfaceOrClassHelper.FindComponentOfInterfaceOrClass(out Notifier);
            var cs = new NativeArray<ComponentType>(6, Allocator.Temp)
            {
                [0] = ComponentType.ReadOnly<Translation>(),
                [1] = ComponentType.ReadOnly<Rotation>(),
                [2] = ComponentType.ReadOnly<PhysicsVelocity>(),
                [3] = ComponentType.ReadOnly<PlayerMachineTag>(),
                [4] = ComponentType.ReadOnly<TeamTag>(),
                [5] = ComponentType.ReadOnly<PhysicsMass>(),
            };
            RequireForUpdate(_query = GetEntityQuery(cs));
            cs.Dispose();
        }

        protected override void OnStartRunning()
        {
            var actor = View.OwnerActorNr;
            using (var array = _query.ToEntityArray(Allocator.TempJob))
            {
                for (var i = 0; i < array.Length; i++)
                {
                    if (EntityManager.GetComponentData<TeamTag>(array[i]).Id != actor) continue;
                    PlayerEntity = array[i];
                    break;
                }
            }
        }

        protected override void OnUpdate()
        {
            var manager = World.Active.EntityManager;
            if (!manager.Exists(PlayerEntity)) return;
            var keyW = Input.GetKey(KeyCode.W);
            var keyA = Input.GetKey(KeyCode.A);
            var keyS = Input.GetKey(KeyCode.S);
            var keyD = Input.GetKey(KeyCode.D);
            var keyShift = Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift);
            //if (!keyW && !keyA && !keyS && !keyD && !keyShift) return;
            var physicsVelocity = manager.GetComponentData<PhysicsVelocity>(PlayerEntity);
            var rotation = manager.GetComponentData<Rotation>(PlayerEntity);
        }
    }
}
