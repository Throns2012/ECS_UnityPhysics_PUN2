using System;
using System.Collections.Generic;
using Assets.MyFolder.Scripts;
using Assets.MyFolder.Scripts.Basics;
using Assets.MyFolder.Scripts.Managers_and_Systems;
using Assets.MyFolder.Scripts.Utility;
using Photon.Pun;
using Photon.Realtime;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[RequireComponent(typeof(Camera))]
public class Initializer : MonoBehaviourPunCallbacks, IMoveNotifier, ISynchronizer, IPrefabStorage, IPointAppearNotifier
{
    private PhotonViewExtension _view;
    private readonly List<Entity> _prefabs = new List<Entity>();

    private bool _amIMaster;
    private bool AmIMaster
    {
        get => _amIMaster;
        set => _amIMaster = World.Active.GetOrCreateSystem<PointAppearSystem>().Enabled = value;
    }

    public Vector3 Min, Max;
    public int PointMin, PointMax;

    private void Start()
    {
        if (!PhotonNetwork.ConnectUsingSettings()) throw new ApplicationException();
        var world = World.Active;
        world.GetOrCreateSystem<ControlPlayerMachineSystem>().Notifier = this;
        world.GetOrCreateSystem<FrameIntervalSyncStarterSystem>().Synchronizer = this;
        world.GetOrCreateSystem<ManualCameraSystem>().MainCamera
            = world.GetOrCreateSystem<FollowingCameraSystem>().MainCamera
            = GetComponent<Camera>();
        var pointAppearSystem = world.GetOrCreateSystem<PointAppearSystem>();
        pointAppearSystem.Notifier = this;
        pointAppearSystem.PointMax = PointMax;
        pointAppearSystem.PointMin = PointMin;
        var confineSystem = world.GetOrCreateSystem<ConfineSystem>();
        pointAppearSystem.Max = confineSystem.Max = Max;
        pointAppearSystem.Min = confineSystem.Min = Min;
    }

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.JoinOrCreateRoom("DEFAULT", new RoomOptions(), TypedLobby.Default))
            throw new ApplicationException();
    }

    public override void OnJoinedRoom()
    {
        var rand = new Random((uint)DateTime.Now.Ticks);
        var instantiatedObj = PhotonNetwork.Instantiate("Photon View Directional Light", rand.NextFloat3(new float3(1f, -0.1f, 1f) * -10f, new float3(1, 1, 1) * 10), Quaternion.identity);
        var world = World.Active;
        world.GetExistingSystem<FollowingCameraSystem>().View =
            world.GetExistingSystem<FrameIntervalSyncStarterSystem>().View =
            _view =
            instantiatedObj.GetComponent<PhotonViewExtension>();
        AmIMaster = PhotonNetwork.MasterClient.ActorNumber == _view.OwnerActorNr;
    }

    public override void OnLeftRoom()
    {
        _prefabs.Clear();
    }

    public bool FindPrefab<T>(EntityManager manager, out Entity prefabEntity)
        where T : struct, IComponentData
    {
        foreach (var prefab in _prefabs)
        {
            if (!manager.HasComponent<T>(prefab)) continue;
            prefabEntity = prefab;
            return true;
        }
        prefabEntity = Entity.Null;
        return false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!AmIMaster) return;
        //_view.RPC(nameof(PhotonViewExtension.InitializeEverythingInternal), newPlayer);
    }

    public override void OnMasterClientSwitched(Player newMasterClient) => AmIMaster = newMasterClient.ActorNumber == _view.OwnerActorNr;

    public void OrderMoveCommand(Vector3 deltaVelocity, DateTime time)
    {
        var objects2 = ArrayPool.Get(2);
        objects2[0] = deltaVelocity;
        objects2[1] = time.Ticks;
        _view.OrderMoveCommandInternal(deltaVelocity, time.Ticks, _view.OwnerActorNr);
        _view.RPC(nameof(PhotonViewExtension.OrderMoveCommandInternal), RpcTarget.Others, objects2);
    }

    public void Sync(in Translation position, in Rotation rotation, in PhysicsVelocity velocity)
    {
        var objects4 = ArrayPool.Get(4);
        objects4[0] = (Vector3)position.Value;
        objects4[1] = (Quaternion)rotation.Value;
        objects4[2] = (Vector3)velocity.Linear;
        objects4[3] = (Vector3)velocity.Angular;
        _view.RPC(nameof(PhotonViewExtension.SyncInternal), RpcTarget.Others, objects4);
    }

    public void Add(Entity entity)
    {
        if (!World.Active.EntityManager.HasComponent<Prefab>(entity))
            World.Active.EntityManager.AddComponentData(entity, new Prefab());
        _prefabs.Add(entity);
    }

    public IEnumerable<Entity> Prefabs => _prefabs;
    private byte[] _nextPointBytes = Array.Empty<byte>();

    public unsafe void NextPoint(NativeArray<Translation> nextTranslations, NativeArray<Point> nextPoints)
    {
        var byteLength = nextTranslations.Length * (sizeof(Translation) + sizeof(Point));
        if (byteLength == 0) return;
        if (_nextPointBytes.Length < byteLength)
            _nextPointBytes = new byte[byteLength];
        fixed (byte* ptr = &_nextPointBytes[0])
        {
            UnsafeUtility.MemCpy(ptr, nextTranslations.GetUnsafePtr(), sizeof(Translation) * nextTranslations.Length);
            UnsafeUtility.MemCpy(ptr + sizeof(Translation) * nextTranslations.Length, nextPoints.GetUnsafePtr(), sizeof(Point) * nextPoints.Length);
        }
        var objects1 = ArrayPool.Get(1);
        objects1[0] = _nextPointBytes;
        _view.RPC(nameof(PhotonViewExtension.NextPointPointsInternal), RpcTarget.All, objects1);
    }
}
