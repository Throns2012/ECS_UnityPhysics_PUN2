using System.Collections.Generic;
using Assets.MyFolder.Scripts.Basics;
using Assets.MyFolder.Scripts.Managers_and_Systems;
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
    public IInitialDeserializer InitialDeserializer;
    private Entity _nextPointPrefabEntity;
    private Entity _playerEntity;
    private EntityArchetype _moveCommandComponentTypes;
    private EntityArchetype _synchronizePositionComponentTypes;

    void OnEnable()
    {
        var manager = World.Active.EntityManager;

        var moveCommandComponentTypes = ArrayPool.Get<ComponentType>(3);
        moveCommandComponentTypes[0] = ComponentType.ReadWrite<MoveCommand>();
        moveCommandComponentTypes[1] = ComponentType.ReadWrite<DestroyableComponentData>();
        moveCommandComponentTypes[2] = ComponentType.ReadWrite<DateTimeTicksToProcess>();

        _moveCommandComponentTypes = manager.CreateArchetype(moveCommandComponentTypes);

        var synchronizePositionComponentTypes = ArrayPool.Get<ComponentType>(6);
        synchronizePositionComponentTypes[0] = ComponentType.ReadWrite<SyncInfoTag>();
        synchronizePositionComponentTypes[1] = ComponentType.ReadWrite<TeamTag>();
        synchronizePositionComponentTypes[2] = ComponentType.ReadWrite<Translation>();
        synchronizePositionComponentTypes[3] = ComponentType.ReadWrite<Rotation>();
        synchronizePositionComponentTypes[4] = ComponentType.ReadWrite<PhysicsVelocity>();
        synchronizePositionComponentTypes[5] = ComponentType.ReadWrite<DestroyableComponentData>();

        _synchronizePositionComponentTypes = manager.CreateArchetype(synchronizePositionComponentTypes);

        if (!FindComponentOfInterfaceOrClassHelper.FindComponentOfInterfaceOrClass(out Storage)) throw new KeyNotFoundException();
        Storage.FindPrefab<PlayerMachineTag>(manager, out var playerPrefabEntity);
        InstantiatePlayerPrefab(playerPrefabEntity);
        Storage.FindPrefab<Point, DateTimeTicksToProcess>(manager, out _nextPointPrefabEntity);
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

    internal void OrderMoveCommandInternal(Vector3 deltaVelocity, long ticks, int actorNumber)
    {
        var entityManager = World.Active.EntityManager;
        var entity = entityManager.CreateEntity(_moveCommandComponentTypes);
        entityManager.SetComponentData(entity, new MoveCommand()
        {
            Id = actorNumber,
            DeltaVelocity = deltaVelocity,
        });
        entityManager.SetComponentData(entity, new DateTimeTicksToProcess(ticks));
    }

    [PunRPC]
    internal void OrderMoveCommandInternal(Vector3 deltaVelocity, long ticks, PhotonMessageInfo msgInfo)
        => OrderMoveCommandInternal(deltaVelocity, ticks, msgInfo.Sender.ActorNumber);

    internal void SyncInternal(Vector3 position, Quaternion rotation, Vector3 linear, Vector3 angular, int actorNumber, int milliseconds)
    {
        var entityManager = World.Active?.EntityManager;
        if (entityManager is null) return;
        var entity = entityManager.CreateEntity(_synchronizePositionComponentTypes);
        var msgInfoSentServerTimestamp = milliseconds;
        entityManager.SetComponentData(entity, new SyncInfoTag { SentServerTimestamp = msgInfoSentServerTimestamp });
        entityManager.SetComponentData(entity, new TeamTag { Id = actorNumber });
        entityManager.SetComponentData(entity, new Translation { Value = position });
        entityManager.SetComponentData(entity, new Rotation { Value = rotation });
        entityManager.SetComponentData(entity, new PhysicsVelocity
        {
            Linear = linear,
            Angular = angular,
        });
    }

    [PunRPC]
    internal void SyncInternal(Vector3 position, Quaternion rotation, Vector3 linear, Vector3 angular, PhotonMessageInfo msgInfo)
        => SyncInternal(position, rotation, linear, angular, msgInfo.Sender.ActorNumber, msgInfo.SentServerTimestamp);

    [PunRPC]
    internal unsafe void NextPointsInternal(byte[] binaryBytes)
    {
        var count = (binaryBytes.Length - sizeof(long)) / (sizeof(Point) + sizeof(Translation));
        if (count == 0) return;
        var manager = World.Active?.EntityManager;
        if (manager is null) return;
        if (!manager.Exists(_nextPointPrefabEntity)) return;
        using (var entities = new NativeArray<Entity>(count, Allocator.Temp, NativeArrayOptions.UninitializedMemory))
        {
            fixed (byte* ptr = &binaryBytes[0])
            {
                var dateTimeTicksToProcess = new DateTimeTicksToProcess(*(long*)ptr);
                manager.SetComponentData(_nextPointPrefabEntity, dateTimeTicksToProcess);
                manager.Instantiate(_nextPointPrefabEntity, entities);
                var translationPtr = (Translation*)(ptr + sizeof(long));
                var pointPtr = (Point*)(translationPtr + count);
                for (var i = 0; i < entities.Length; i++)
                {
                    manager.SetComponentData(entities[i], translationPtr[i]);
                    manager.SetComponentData(entities[i], pointPtr[i]);
                }
            }
        }
    }

    [PunRPC]
    internal unsafe void InitializeEverythingInternal(byte[] serializedBytes)
    {
        InitialDeserializer.Deserialize(serializedBytes);
    }


}
