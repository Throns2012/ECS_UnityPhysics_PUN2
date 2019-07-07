using System.Collections.Generic;
using System.Linq;
using Assets.MyFolder.Scripts.Basics;
using Assets.MyFolder.Scripts.Utility;
using Photon.Pun;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Material = UnityEngine.Material;
using Random = Unity.Mathematics.Random;

public sealed unsafe class PhotonViewExtension : PhotonView
{
    public Material PlayerMaterial;
    private IPrefabStorage _prefabStorage;
    private IInitialDeserializer _initialDeserializer;
    private Entity _nextPointPrefabEntity;
    private Entity _playerEntity;
    private Entity _playerPrefabEntity;
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

        if (!FindComponentOfInterfaceOrClassHelper.FindComponentOfInterfaceOrClass(out _prefabStorage)) throw new KeyNotFoundException();
        if (!FindSystemOfInterfaceHelper.FindSystemOfInterface(out _initialDeserializer)) throw new KeyNotFoundException();
        _prefabStorage.FindPrefab<PlayerMachineTag>(manager, out _playerPrefabEntity);
        if (IsMine)
        {
            InstantiatePlayerPrefab(_playerPrefabEntity);
            RPC(nameof(SpawnPlayerMachine), RpcTarget.Others);
        }
        _prefabStorage.FindPrefab<Point, DateTimeTicksToProcess>(manager, out _nextPointPrefabEntity);
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


        var tmpRenderer = entityManager.GetSharedComponentData<RenderMesh>(_playerEntity);
        tmpRenderer.material = PlayerMaterial;
        entityManager.SetSharedComponentData(_playerEntity, tmpRenderer);

        var idEntity = entityManager.CreateEntity(ComponentType.ReadWrite<UserIdSingleton>());
        var userIdSingleton = new UserIdSingleton(OwnerActorNr, _playerEntity);
        entityManager.SetComponentData(idEntity, userIdSingleton);
    }

    internal void OrderMoveCommandInternal(byte[] serializedBytes, int actorNumber)
    {
        var entityManager = World.Active.EntityManager;
        var entity = entityManager.CreateEntity(_moveCommandComponentTypes);
        fixed (byte* ptr = &serializedBytes[0])
        {
            entityManager.SetComponentData(entity, new MoveCommand()
            {
                Id = actorNumber,
                DeltaVelocity = *(float3*)ptr,
            });
            entityManager.SetComponentData(entity, *(DateTimeTicksToProcess*)(ptr + sizeof(float3)));
        }
    }

    [PunRPC]
    internal void OrderMoveCommandInternal(byte[] serializedBytes, PhotonMessageInfo msgInfo)
    // Vector3 -> 16
    // long -> 9
    // byte[4 * 3 + 8] -> 5 + 20 = 25
        => OrderMoveCommandInternal(serializedBytes, msgInfo.Sender.ActorNumber);

    internal void SyncInternal(byte[] serializedBytes, int actorNumber, int milliseconds)
    {
        var entityManager = World.Active?.EntityManager;
        if (entityManager is null) return;
        var entity = entityManager.CreateEntity(_synchronizePositionComponentTypes);
        entityManager.SetComponentData(entity, *(SyncInfoTag*)&milliseconds);
        entityManager.SetComponentData(entity, *(TeamTag*)&actorNumber);
        fixed (byte* ptr = &serializedBytes[0])
        {
            entityManager.SetComponentData(entity, *(Translation*)ptr);
            entityManager.SetComponentData(entity, *(Rotation*)(ptr + sizeof(Translation)));
            entityManager.SetComponentData(entity, *(PhysicsVelocity*)(ptr + sizeof(Translation) + sizeof(Rotation)));
        }
    }

    [PunRPC]
    internal void SyncInternal(byte[] serializedBytes, PhotonMessageInfo msgInfo)
    // Vector3 -> 16
    // Quaternion -> 20
    // 16 * 3 + 20 = 68bytes
    // byte[4*3*3+4*4] -> 5 + 4 * (9 + 4) = 57bytes
        => SyncInternal(serializedBytes, msgInfo.Sender.ActorNumber, msgInfo.SentServerTimestamp);

    [PunRPC]
    internal void NextPointsInternal(byte[] binaryBytes)
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
    internal void InitializeEverythingInternal(byte[] serializedBytes)
    {
        _initialDeserializer.Deserialize(serializedBytes);
    }

    public List<int> commands = new List<int>();
    private readonly int COMMAND_MAX_LEN = 4;
    [PunRPC]
    internal void SpawnPlayerMachine(PhotonMessageInfo msgInfo)
    {
        commands.AddRange(Enumerable.Repeat(0, 4).Select(_ => UnityEngine.Random.Range(0, COMMAND_MAX_LEN)));
        var manager = World.Active.EntityManager;
        var entity = manager.Instantiate(_playerPrefabEntity);
        manager.SetComponentData(entity, new TeamTag() { Id = msgInfo.Sender.ActorNumber });
    }
}
