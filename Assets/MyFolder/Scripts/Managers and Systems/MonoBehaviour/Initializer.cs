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
    private IInitialSerializer _initialSerializer;
    private readonly object[] _objects1 = ArrayPool.Get(1);
    private readonly byte[] _bytes25 = ArrayPool.Get<byte>(25);
    private readonly byte[] _bytes52 = ArrayPool.Get<byte>(52);

    private bool _amIMaster;
    private bool AmIMaster
    {
        get => _amIMaster;
        set => _amIMaster = World.Active.GetOrCreateSystem<DecideNextPointsSystem>().Enabled = value;
    }

    public Vector3 Min, Max;
    public int PointMin, PointMax;

    private bool ExistOthers => (PhotonNetwork.CurrentRoom?.Players?.Count ?? 0) > 1;

    private void Start()
    {
        if (!PhotonNetwork.ConnectUsingSettings()) throw new ApplicationException();

        var world = World.Active;

        world.GetOrCreateSystem<ControlPlayerMachineAbsoluteXyzSystem>().Notifier = this;

        var controlPlayerMachineRelativeToCameraSystem = world.GetOrCreateSystem<ControlPlayerMachineRelativeToCameraSystem>();
        controlPlayerMachineRelativeToCameraSystem.Notifier = this;
        controlPlayerMachineRelativeToCameraSystem.Enabled = false;

        world.GetOrCreateSystem<TicksIntervalSyncSystem>().Synchronizer = this;

        world.GetOrCreateSystem<ManualCameraSystem>().MainCamera
            = world.GetOrCreateSystem<FollowingCameraSystem>().MainCamera
            = GetComponent<Camera>();

        var pointAppearSystem = world.GetOrCreateSystem<DecideNextPointsSystem>();
        pointAppearSystem.Notifier = this;
        pointAppearSystem.PointMax = PointMax;
        pointAppearSystem.PointMin = PointMin;

        var confineSystem = world.GetOrCreateSystem<ConfineSystem>();
        pointAppearSystem.Max = confineSystem.Max = Max;
        pointAppearSystem.Min = confineSystem.Min = Min;

        var manager = world.EntityManager;
        FindPrefab<Point, DestroyableComponentData>(manager, out var currentPointPrefabEntity);
        world.GetOrCreateSystem<EnableWhenTimeComesSystem>().CurrentPointPrefab = currentPointPrefabEntity;

        var serializeEverythingSystem = world.GetOrCreateSystem<SerializeEverythingSystem>();
        _initialSerializer = serializeEverythingSystem;
        serializeEverythingSystem.CurrentPointPrefabEntity = currentPointPrefabEntity;
        FindPrefab<Point, DateTimeTicksToProcess>(manager, out serializeEverythingSystem.NextPointPrefabEntity);
        FindPrefab<PlayerMachineTag>(manager, out serializeEverythingSystem.PlayerMachinePrefabEntity);
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
        world.GetExistingSystem<TicksIntervalSyncSystem>().View =
        world.GetExistingSystem<ControlPlayerMachineMoreComplexSystem>().View =
        world.GetExistingSystem<CollisionDetectionSystemBetweenPlayerAndPoint>().View =
        world.GetExistingSystem<RespawnSystem>().View =
        world.GetExistingSystem<FindPlayerEntityHelper>().View =
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

    public bool FindPrefab<T0, T1>(EntityManager manager, out Entity prefabEntity)
        where T0 : struct, IComponentData
        where T1 : struct, IComponentData
    {
        foreach (var prefab in _prefabs)
        {
            if (!manager.HasComponent<T0>(prefab) || !manager.HasComponent<T1>(prefab)) continue;
            prefabEntity = prefab;
            return true;
        }
        prefabEntity = Entity.Null;
        return false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!AmIMaster || newPlayer.ActorNumber == _view.OwnerActorNr) return;
        _objects1[0] = _initialSerializer.Serialize();
        _view.RPC(nameof(PhotonViewExtension.InitializeEverythingInternal), newPlayer, _objects1);
    }

    public override void OnMasterClientSwitched(Player newMasterClient) => AmIMaster = newMasterClient.ActorNumber == _view.OwnerActorNr;

    public unsafe void OrderMoveCommand(Vector3 deltaVelocity, DateTime time)
    {
        fixed (byte* ptr = &_bytes25[0])
        {
            *(Vector3*)ptr = deltaVelocity;
            *(DateTime*)(ptr + sizeof(Vector3)) = time;
        }
        _view.OrderMoveCommandInternal(_bytes25, _view.OwnerActorNr);
        if (!ExistOthers) return;
        _objects1[0] = _bytes25;
        _view.RPC(nameof(PhotonViewExtension.OrderMoveCommandInternal), RpcTarget.Others, _objects1);
    }

    public unsafe void Sync(in Translation position, in Rotation rotation, in PhysicsVelocity velocity)
    {
        if (!ExistOthers) return;
        fixed (byte* ptr = &_bytes52[0])
        {
            *(Translation*)ptr = position;
            *(Rotation*)(ptr + sizeof(Translation)) = rotation;
            *(PhysicsVelocity*)(ptr + sizeof(Translation) + sizeof(Rotation)) = velocity;
        }
        _objects1[0] = _bytes52;
        _view.RPC(nameof(PhotonViewExtension.SyncInternal), RpcTarget.Others, _objects1);
    }

    public void Add(Entity entity)
    {
        if (!World.Active.EntityManager.HasComponent<Prefab>(entity))
            World.Active.EntityManager.AddComponentData(entity, new Prefab());
        _prefabs.Add(entity);
    }

    public IEnumerable<Entity> Prefabs => _prefabs;
    private byte[] _nextPointBytes = Array.Empty<byte>();

    public unsafe void NextPoint(NativeArray<Translation> nextTranslations, NativeArray<Point> nextPoints, DateTime time)
    {
        var byteLength = nextTranslations.Length * (sizeof(Translation) + sizeof(Point));
        if (byteLength == 0) return;
        byteLength += 8;
        if (_nextPointBytes.Length < byteLength)
            _nextPointBytes = new byte[byteLength];
        fixed (byte* ptr = &_nextPointBytes[0])
        {
            *(long*)ptr = time.Ticks;
            UnsafeUtility.MemCpy(ptr + sizeof(long), nextTranslations.GetUnsafePtr(), sizeof(Translation) * nextTranslations.Length);
            UnsafeUtility.MemCpy(ptr + sizeof(long) + sizeof(Translation) * nextTranslations.Length, nextPoints.GetUnsafePtr(), sizeof(Point) * nextPoints.Length);
        }
        _view.NextPointsInternal(_nextPointBytes);
        if (!ExistOthers) return;
        _objects1[0] = _nextPointBytes;
        _view.RPC(nameof(PhotonViewExtension.NextPointsInternal), RpcTarget.Others, _objects1);
    }
}
