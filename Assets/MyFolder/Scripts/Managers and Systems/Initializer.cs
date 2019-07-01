using System;
using Assets.MyFolder.Scripts;
using Photon.Pun;
using Photon.Realtime;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class Initializer : MonoBehaviourPunCallbacks, IMoveNotifier
{
    private PhotonViewExtension _view;

    private void Start()
    {
        if (!PhotonNetwork.ConnectUsingSettings()) throw new ApplicationException();
        World.Active.GetOrCreateSystem<Controller>().Notifier = this;
        _objArrayOrderMoveCommand = new object[1];
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("DEFAULT", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Random rand = new Random((uint)DateTime.Now.Ticks);
        var instantiatedObj = PhotonNetwork.Instantiate("Photon View Directional Light", rand.NextFloat3(new float3(1f, -0.1f, 1f) * -10f, new float3(1, 1, 1) * 10), Quaternion.identity);
        _view = instantiatedObj.GetComponent<PhotonViewExtension>();
    }

    private object[] _objArrayOrderMoveCommand;

    public void OrderMoveCommand(Vector3 deltaVelocity)
    {
        _objArrayOrderMoveCommand[0] = deltaVelocity;
        _view.RPC(nameof(PhotonViewExtension.OrderMoveCommandInternal), RpcTarget.All, _objArrayOrderMoveCommand);
    }
}
