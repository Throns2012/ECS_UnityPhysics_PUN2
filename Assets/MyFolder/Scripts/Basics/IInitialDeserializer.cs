using Unity.Collections;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public interface IInitialDeserializer
    {
        void Deserialize(byte[] serializedBytes);
    }
}
