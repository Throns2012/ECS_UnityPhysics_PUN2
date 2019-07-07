using System.Collections.Generic;
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
    public sealed class RespawnSystem : ComponentSystem
    {
        private IPlayerGenerator _playerGenerator;
        private Entity _playerEntity;
        private FindPlayerEntityHelper _findPlayerEntityHelper;
        public PhotonView View;

        protected override void OnCreate()
        {
            var cs = new NativeArray<ComponentType>(5, Allocator.Temp)
            {
                [0] = ComponentType.ReadOnly<Translation>(),
                [1] = ComponentType.ReadOnly<Rotation>(),
                [2] = ComponentType.ReadOnly<PhysicsVelocity>(),
                [3] = ComponentType.ReadOnly<PlayerMachineTag>(),
                [4] = ComponentType.ReadOnly<TeamTag>(),
            };
            GetEntityQuery(cs);
            cs.Dispose();
            FindComponentOfInterfaceOrClassHelper.FindComponentOfInterfaceOrClass(out _playerGenerator);
            _findPlayerEntityHelper = World.Active.GetOrCreateSystem<FindPlayerEntityHelper>();
            RequireSingletonForUpdate<UserIdSingleton>();
        }

        protected override void OnStartRunning()
        {
            if (_playerGenerator == null && !FindComponentOfInterfaceOrClassHelper.FindComponentOfInterfaceOrClass(out _playerGenerator))
                return;
            if (_playerEntity == Entity.Null)
            {
                _playerEntity = _playerGenerator.PlayerInstantiate(View.OwnerActorNr);
                return;
            }
            if (EntityManager.Exists(_playerEntity) || View == null)
                return;
            _playerEntity = _findPlayerEntityHelper.Find();
            if (EntityManager.Exists(_playerEntity))
                return;
            _playerEntity = _playerGenerator.PlayerInstantiate(View.OwnerActorNr);
        }

        protected override void OnStopRunning()
        {
            Debug.Log("MGOEI");
            OnStartRunning();
        }

        protected override void OnUpdate() => OnStartRunning();
    }
}