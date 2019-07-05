using System;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public readonly struct DateTimeTicksToProcess : IComponentData, IEquatable<DateTimeTicksToProcess>
    {
        public readonly long Value;
        public DateTimeTicksToProcess(long ticks) => Value = ticks;

        public bool Equals(DateTimeTicksToProcess other) => Value == other.Value;

        public override bool Equals(object obj) => obj is DateTimeTicksToProcess other && Equals(other);

        public override int GetHashCode() => (int)Value ^ (int)(Value >> 32);
    }
}
