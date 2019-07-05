using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public struct SyncInfoTag : IComponentData
    {
        public int SentServerTimestamp;
    }
}