using Assets.MyFolder.Scripts;
using Assets.MyFolder.Scripts.Basics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public sealed class Controller : ComponentSystem
{
    public IMoveNotifier Notifier;

    protected override void OnCreate()
    {
        GetEntityQuery(new[]
        {
            ComponentType.ReadOnly<UserIdSingleton>(),
        });
    }

    protected override void OnUpdate()
    {
        if (NoInput(out var keyW, out var keyA, out var keyS, out var keyD, out var keyShift)) return;
        var delta = CalculateDeltaVelocity(keyW, keyS, keyA, keyD, keyShift);
        Notifier.OrderMoveCommand(delta);
    }

    private static float3 CalculateDeltaVelocity(bool keyW, bool keyS, bool keyA, bool keyD, bool keyShift)
    {
        float3 delta = default;
        if (keyW)
            delta.z += 10f;
        if (keyS)
            delta.z -= 10f;
        if (keyA)
            delta.x -= 10f;
        if (keyD)
            delta.x += 10f;
        if (keyShift)
            delta.y += 20f;
        delta *= Time.deltaTime;
        return delta;
    }

    private static bool NoInput(out bool keyW, out bool keyA, out bool keyS, out bool keyD, out bool keyShift)
    {
        keyW = Input.GetKey(KeyCode.W) | Input.GetKey(KeyCode.UpArrow);
        keyA = Input.GetKey(KeyCode.A) | Input.GetKey(KeyCode.LeftArrow);
        keyS = Input.GetKey(KeyCode.S) | Input.GetKey(KeyCode.DownArrow);
        keyD = Input.GetKey(KeyCode.D) | Input.GetKey(KeyCode.RightArrow);
        keyShift = Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift);
        return !keyW && !keyA && !keyS && !keyD && !keyShift;
    }
}
