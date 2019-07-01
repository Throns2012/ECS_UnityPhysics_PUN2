using Assets.MyFolder.Scripts.Basics;
using Photon.Pun;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public sealed class PhotonViewExtension : PhotonView
{
    public Material PlayerMaterial;

    void Start()
    {
        _moveCommandComponentTypes = new[] { ComponentType.ReadWrite<MoveCommand>(), };

        var player = transform.GetChild(0).gameObject;

        var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(player, World.Active);
        var entityManager = World.Active.EntityManager;

        var teamTag = new TeamTag()
        {
            Id = OwnerActorNr,
        };
        entityManager.AddComponentData(entity, teamTag);

        if (IsMine)
        {
            var tmpRenderer = entityManager.GetSharedComponentData<RenderMesh>(entity);
            tmpRenderer.material = PlayerMaterial;
            entityManager.SetSharedComponentData(entity, tmpRenderer);

            var idEntity = entityManager.CreateEntity(ComponentType.ReadWrite<UserIdSingleton>());
            var userIdSingleton = new UserIdSingleton(OwnerActorNr);
            entityManager.SetComponentData(idEntity, userIdSingleton);
        }

        Destroy(player);
    }

    private ComponentType[] _moveCommandComponentTypes;

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
}
