using Assets.MyFolder.Scripts.Basics;
using Photon.Pun;
using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Material = UnityEngine.Material;

public sealed class PhotonViewExtension : PhotonView
{
    public Material PlayerMaterial;
    private Entity _playerEntity;
    private ComponentType[] _moveCommandComponentTypes;
    private ComponentType[] _synchronizePositionComponentTypes;

    void Start()
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

        var player = transform.GetChild(0).gameObject;

        _playerEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(player, World.Active);
        var entityManager = World.Active.EntityManager;

        var teamTag = new TeamTag()
        {
            Id = OwnerActorNr,
        };
        entityManager.AddComponentData(_playerEntity, teamTag);

        if (IsMine)
        {
            var tmpRenderer = entityManager.GetSharedComponentData<RenderMesh>(_playerEntity);
            tmpRenderer.material = PlayerMaterial;
            entityManager.SetSharedComponentData(_playerEntity, tmpRenderer);

            var idEntity = entityManager.CreateEntity(ComponentType.ReadWrite<UserIdSingleton>());
            var userIdSingleton = new UserIdSingleton(OwnerActorNr);
            entityManager.SetComponentData(idEntity, userIdSingleton);
        }
        Destroy(player);
    }

    private void OnDestroy()
    {
        if (World.Active is null) return;
        var manager = World.Active.EntityManager;
        if (manager is null) return;
        if (!manager.Exists(_playerEntity)) return;
        manager.SetComponentData(_playerEntity, new DestroyableComponentData()
        {
            ShouldDestroy = true
        });
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
}
