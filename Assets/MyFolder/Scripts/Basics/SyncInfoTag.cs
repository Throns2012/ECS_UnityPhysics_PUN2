using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Assets.MyFolder.Scripts.Basics
{
    public struct SyncInfoTag : IComponentData
    {
        public int SentServerTimestamp;
    }
}