using System;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public readonly struct UserIdSingleton : IComponentData
    {
        public readonly int Id;

        public UserIdSingleton(int id)
        {
            Id = id;
        }
    }
}
