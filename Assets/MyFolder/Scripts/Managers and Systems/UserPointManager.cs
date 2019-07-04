using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed unsafe class UserPointManager : ComponentSystem, IDisposable
    {
        private int* _pointPtr;
        public ref int Point => ref *_pointPtr;
        public int* PointPtr => _pointPtr;

        protected override void OnCreate()
        {
            _pointPtr = (int*)UnsafeUtility.Malloc(sizeof(int), 4, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (_pointPtr == null) return;
            UnsafeUtility.Free(_pointPtr, Allocator.Persistent);
            _pointPtr = null;
        }

        protected override void OnUpdate() { }
    }
}
