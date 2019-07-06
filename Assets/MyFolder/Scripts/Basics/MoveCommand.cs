using Unity.Entities;
using Unity.Mathematics;

namespace Assets.MyFolder.Scripts.Basics
{
    public struct MoveCommand : IComponentData
    {
        public int Id;
        public float3 DeltaVelocity;
    }
}
