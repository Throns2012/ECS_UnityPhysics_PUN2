using System;
using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics
{
    public struct Point : IComponentData, IEquatable<Point>
    {
        public int Value;

        public Point(int value) => Value = value;

        public bool Equals(Point other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Point other && Equals(other);
        public override int GetHashCode() => Value;
    }

    public sealed class PointComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Value;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (dstManager.HasComponent<Point>(entity))
                dstManager.SetComponentData(entity, new Point(Value));
            else dstManager.AddComponentData(entity, new Point(Value));
        }
    }
}
