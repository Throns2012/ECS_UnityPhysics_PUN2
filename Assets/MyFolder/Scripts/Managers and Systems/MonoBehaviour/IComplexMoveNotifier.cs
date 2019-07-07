using System;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts
{
    public interface IComplexMoveNotifier
    {
        void Notify(Translation translation, Rotation rotation, PhysicsVelocity velocity, DateTime time);
    }
}
