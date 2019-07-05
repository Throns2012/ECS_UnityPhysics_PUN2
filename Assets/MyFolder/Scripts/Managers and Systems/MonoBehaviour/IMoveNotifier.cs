using System;
using UnityEngine;

namespace Assets.MyFolder.Scripts
{
    public interface IMoveNotifier
    {
        void OrderMoveCommand(Vector3 deltaVelocity, DateTime time);
    }
}