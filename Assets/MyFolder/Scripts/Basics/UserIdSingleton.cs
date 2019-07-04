using System;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public readonly struct UserIdSingleton : IComponentData, IEquatable<UserIdSingleton>
    {
        public readonly int Id;
        public readonly Entity UserMachineEntity;

        public UserIdSingleton(int id, Entity userMachine)
        {
            Id = id;
            UserMachineEntity = userMachine;
        }

        public bool Equals(UserIdSingleton other) => Id == other.Id;

        public override bool Equals(object obj) => obj is UserIdSingleton other && Equals(other);

        public override int GetHashCode() => Id;
    }
}
