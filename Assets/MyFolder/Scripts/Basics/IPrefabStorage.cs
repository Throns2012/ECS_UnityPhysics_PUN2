using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public interface IPrefabStorage
    {
        void Add(Entity entity);
        IEnumerable<Entity> Prefabs { get; }
    }
}
