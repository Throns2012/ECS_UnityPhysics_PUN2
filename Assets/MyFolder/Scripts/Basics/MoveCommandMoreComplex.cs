using Unity.Entities;
using Unity.Physics;

namespace Assets.MyFolder.Scripts.Basics
{
    public struct MoveCommandMoreComplex : IComponentData
    {
        public PhysicsVelocity Value;
    }
}
