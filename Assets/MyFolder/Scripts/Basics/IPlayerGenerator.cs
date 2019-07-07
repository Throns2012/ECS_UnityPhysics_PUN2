using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public interface IPlayerGenerator
    {
        Entity PlayerInstantiate(int id);
    }
}
