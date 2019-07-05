using Unity.Physics;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts
{
    public interface ISynchronizer
    {
        void Sync(in Translation position, in Rotation rotation, in PhysicsVelocity velocity);
    }
}
