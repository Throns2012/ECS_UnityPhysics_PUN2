using System.Collections.Generic;
using Assets.MyFolder.Scripts.Basics;
using Assets.MyFolder.Scripts.Utility;
using Photon.Pun;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Material = UnityEngine.Material;

public sealed class PhotonViewExtension : PhotonView
{
    public Material PlayerMaterial;
    public IPrefabStorage Storage;
    private Entity _pointPrefabEntity;
    private Entity _playerEntity;
    private ComponentType[] _moveCommandComponentTypes;
    private ComponentType[] _synchronizePositionComponentTypes;

    void OnEnable()
    {
        _moveCommandComponentTypes = new[] { ComponentType.ReadWrite<MoveCommand>(), ComponentType.ReadWrite<DestroyableComponentData>(), };
        _synchronizePositionComponentTypes = new[]
        {
            ComponentType.ReadWrite<SyncInfoTag>(),
            ComponentType.ReadWrite<TeamTag>(),
            ComponentType.ReadWrite<Translation>(),
            ComponentType.ReadWrite<Rotation>(),
            ComponentType.ReadWrite<PhysicsVelocity>(),
            ComponentType.ReadWrite<DestroyableComponentData>(),
        };

        if (!FindComponentOfInterfaceOrClassHelper.FindComponentOfInterfaceOrClass(out Storage)) throw new KeyNotFoundException();
        Storage.FindPrefab<PlayerMachineTag>(World.Active.EntityManager, out var playerPrefabEntity);
        InstantiatePlayerPrefab(playerPrefabEntity);
        Storage.FindPrefab<Point>(World.Active.EntityManager, out _pointPrefabEntity);
    }

    private void OnDestroy()
    {
        var manager = World.Active?.EntityManager;
        if (manager is null) return;
        if (!manager.Exists(_playerEntity)) return;
        manager.SetComponentData(_playerEntity, new DestroyableComponentData()
        {
            ShouldDestroy = true
        });
    }

    private void InstantiatePlayerPrefab(Entity prefab)
    {
        var entityManager = World.Active.EntityManager;
        _playerEntity = entityManager.Instantiate(prefab);

        var teamTag = new TeamTag()
        {
            Id = OwnerActorNr,
        };
        entityManager.SetComponentData(_playerEntity, teamTag);

        if (IsMine)
        {
            var tmpRenderer = entityManager.GetSharedComponentData<RenderMesh>(_playerEntity);
            tmpRenderer.material = PlayerMaterial;
            entityManager.SetSharedComponentData(_playerEntity, tmpRenderer);

            var idEntity = entityManager.CreateEntity(ComponentType.ReadWrite<UserIdSingleton>());
            var userIdSingleton = new UserIdSingleton(OwnerActorNr, _playerEntity);
            entityManager.SetComponentData(idEntity, userIdSingleton);
        }
    }

    [PunRPC]
    internal void OrderMoveCommandInternal(Vector3 deltaVelocity, PhotonMessageInfo msgInfo)
    {
        var entityManager = World.Active.EntityManager;
        var entity = entityManager.CreateEntity(_moveCommandComponentTypes);
        entityManager.SetComponentData(entity, new MoveCommand()
        {
            Id = msgInfo.Sender.ActorNumber,
            DeltaVelocity = deltaVelocity,
        });
    }

    [PunRPC]
    internal void SyncInternal(Vector3 position, Quaternion rotation, Vector3 linear, Vector3 angular, PhotonMessageInfo msgInfo)
    {
        var entityManager = World.Active?.EntityManager;
        if (entityManager is null || _synchronizePositionComponentTypes is null) return;
        var entity = entityManager.CreateEntity(_synchronizePositionComponentTypes);
        var msgInfoSentServerTimestamp = msgInfo.SentServerTimestamp;
        entityManager.SetComponentData(entity, new SyncInfoTag { SentServerTimestamp = msgInfoSentServerTimestamp });
        entityManager.SetComponentData(entity, new TeamTag { Id = msgInfo.Sender.ActorNumber });
        entityManager.SetComponentData(entity, new Translation { Value = position });
        entityManager.SetComponentData(entity, new Rotation { Value = rotation });
        entityManager.SetComponentData(entity, new PhysicsVelocity
        {
            Linear = linear,
            Angular = angular,
        });
    }

    [PunRPC]
    internal void SyncInternal(SyncInfo syncInfo, PhotonMessageInfo msgInfo)
    {
        var entityManager = World.Active?.EntityManager;
        if (entityManager is null || _synchronizePositionComponentTypes is null) return;
        var entity = entityManager.CreateEntity(_synchronizePositionComponentTypes);
        var msgInfoSentServerTimestamp = msgInfo.SentServerTimestamp;
        entityManager.SetComponentData(entity, new SyncInfoTag { SentServerTimestamp = msgInfoSentServerTimestamp });
        entityManager.SetComponentData(entity, new TeamTag { Id = msgInfo.Sender.ActorNumber });
        entityManager.SetComponentData(entity, new Translation { Value = syncInfo.Position });
        entityManager.SetComponentData(entity, new Rotation { Value = syncInfo.Rotation });
        entityManager.SetComponentData(entity, syncInfo.Velocity);
    }

    [PunRPC]
    internal unsafe void NextPointPointsInternal(byte[] binaryBytes, PhotonMessageInfo msgInfo)
    {
        var count = binaryBytes.Length / (sizeof(Point) + sizeof(Translation));
        if (count == 0) return;
        var manager = World.Active?.EntityManager;
        if (manager is null) return;
        if (!manager.Exists(_pointPrefabEntity)) return;
        using (var entities = new NativeArray<Entity>(count, Allocator.Temp, NativeArrayOptions.UninitializedMemory))
        {
            manager.Instantiate(_pointPrefabEntity, entities);
            fixed (byte* ptr = &binaryBytes[0])
            {
                var translationPtr = (Translation*)ptr;
                var pointPtr = (Point*)(translationPtr + count);
                for (var i = 0; i < entities.Length; i++)
                {
                    manager.SetComponentData(entities[i], translationPtr[i]);
                    manager.SetComponentData(entities[i], pointPtr[i]);
                }
            }
        }
    }
}
