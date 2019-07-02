using System;
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

public unsafe class Initializer : MonoBehaviourPunCallbacks, IMoveNotifier, ISynchronizer
{
    private PhotonViewExtension _view;

    private void Start()
    {
        if (!PhotonNetwork.ConnectUsingSettings()) throw new ApplicationException();
        World.Active.GetOrCreateSystem<Controller>().Notifier = this;
        World.Active.GetOrCreateSystem<FrameIntervalSyncStarterSystem>().Synchronizer = this;
        _objectsLength1 = new object[1];
        _objectsLength4 = new object[4];
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("DEFAULT", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Random rand = new Random((uint)DateTime.Now.Ticks);
        var instantiatedObj = PhotonNetwork.Instantiate("Photon View Directional Light", rand.NextFloat3(new float3(1f, -0.1f, 1f) * -10f, new float3(1, 1, 1) * 10), Quaternion.identity);
        World.Active.GetExistingSystem<FrameIntervalSyncStarterSystem>().View = _view = instantiatedObj.GetComponent<PhotonViewExtension>();
        _syncInfo = new SyncInfo();
    }

    private object[] _objectsLength1, _objectsLength4;

    public void OrderMoveCommand(Vector3 deltaVelocity)
    {
        _objectsLength1[0] = deltaVelocity;
        _view.RPC(nameof(PhotonViewExtension.OrderMoveCommandInternal), RpcTarget.All, _objectsLength1);
    }

    private SyncInfo _syncInfo;

    public void Sync(in Translation position, in Rotation rotation, in PhysicsVelocity velocity)
    {
#if UNITY_EDITOR
        if (_syncInfo is null)
            Debug.LogError($"{nameof(_syncInfo)} is null");
#endif
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
}
