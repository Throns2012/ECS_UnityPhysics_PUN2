using Unity.Entities;

namespace Assets.MyFolder.Scripts.Basics
{
    public static class FindSystemOfInterfaceHelper
    {
        public static bool FindSystemOfInterface<T>(out T system)
            where T : class
        {
            foreach (var @base in World.Active.Systems)
            {
                if (!@base.Enabled) continue;
                system = @base as T;
                if (system is null) continue;
                return true;
            }
            system = null;
            return false;
        }
    }
}
