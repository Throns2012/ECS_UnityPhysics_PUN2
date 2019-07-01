using UnityEngine;

namespace Assets.MyFolder.Scripts
{
    internal interface IPositionSynchronizer
    {
        void SetPosition(int id, Vector3 position);
    }
}
