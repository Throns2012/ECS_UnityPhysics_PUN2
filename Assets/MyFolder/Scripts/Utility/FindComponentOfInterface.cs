using UnityEngine;

namespace Assets.MyFolder.Scripts.Utility
{
    public static class FindComponentOfInterfaceOrClassHelper
    {
        public static bool FindComponentOfInterfaceOrClass<T>(out T component) where T : class
        {
            foreach (var n in Object.FindObjectsOfType<Component>())
            {
                component = n as T;
                if (component is null) continue;
                return true;
            }
            component = null;
            return false;
        }
    }
}