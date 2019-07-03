using System;
using System.Collections.Generic;
using Assets.MyFolder.Scripts;
using Assets.MyFolder.Scripts.Basics;
using Assets.MyFolder.Scripts.Managers_and_Systems;
using Photon.Pun;
using Photon.Realtime;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class Initializer : MonoBehaviourPunCallbacks, IMoveNotifier, ISynchronizer, IPrefabStorage
{
    private PhotonViewExtension _view;
    private List<Entity> _prefabs = new List<Entity>();
    private object[] _objectsLength1 = new object[1], _objectsLength4 = new object[4];
    private SyncInfo _syncInfo = new SyncInfo();

    private void Start()
    {
        if (!PhotonNetwork.ConnectUsingSettings()) throw new ApplicationException();
        World.Active.GetOrCreateSystem<Controller>().Notifier = this;
        World.Active.GetOrCreateSystem<FrameIntervalSyncStarterSystem>().Synchronizer = this;
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("DEFAULT", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        var rand = new Random((uint)DateTime.Now.Ticks);
        var instantiatedObj = PhotonNetwork.Instantiate("Photon View Directional Light", rand.NextFloat3(new float3(1f, -0.1f, 1f) * -10f, new float3(1, 1, 1) * 10), Quaternion.identity);
        var world = World.Active;
        world.GetExistingSystem<FrameIntervalSyncStarterSystem>().View = _view = instantiatedObj.GetComponent<PhotonViewExtension>();
        _view.InstantiatePlayerPrefab(FindPlayerMachinePrefab(world.EntityManager));
    }

    public override void OnLeftRoom()
    {
        _prefabs.Clear();
    }

    private Entity FindPlayerMachinePrefab(EntityManager manager)
    {
        foreach (var prefab in _prefabs)
        {
            if (!manager.HasComponent<PlayerMachineTag>(prefab)) continue;
            return prefab;
        }
        throw new KeyNotFoundException();
    }


    public void OrderMoveCommand(Vector3 deltaVelocity)
    {
        _objectsLength1[0] = deltaVelocity;
        _view.RPC(nameof(PhotonViewExtension.OrderMoveCommandInternal), RpcTarget.All, _objectsLength1);
    }


    public void Sync(in Translation position, in Rotation rotation, in PhysicsVelocity velocity)
    {
        //_objectsLength1[0] = _syncInfo;
        //_syncInfo.Position = position.Value;
        //_syncInfo.Rotation = rotation.Value;
        //_syncInfo.Velocity = velocity;
        //_view.RPC(nameof(PhotonViewExtension.SyncInternal), RpcTarget.Others, _objectsLength1);
        _objectsLength4[0] = (Vector3)position.Value;
        _objectsLength4[1] = (Quaternion)rotation.Value;
        _objectsLength4[2] = (Vector3)velocity.Linear;
        _objectsLength4[3] = (Vector3)velocity.Angular;
        _view.RPC(nameof(PhotonViewExtension.SyncInternal), RpcTarget.All, _objectsLength4);
    }

    public void Add(Entity entity)
    {
        if (!World.Active.EntityManager.HasComponent<Prefab>(entity))
            World.Active.EntityManager.AddComponentData(entity, new Prefab());
        _prefabs.Add(entity);
    }

    public IEnumerable<Entity> Prefabs => _prefabs;
}
