using System;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public struct UserIdSingleton : IComponentData, IEquatable<UserIdSingleton>
    {
        public readonly int Id;

        public UserIdSingleton(int id) => Id = id;

        public bool Equals(UserIdSingleton other) => Id == other.Id;

        public override bool Equals(object obj) => obj is UserIdSingleton other && Equals(other);

        public override int GetHashCode() => Id;
    }
}
