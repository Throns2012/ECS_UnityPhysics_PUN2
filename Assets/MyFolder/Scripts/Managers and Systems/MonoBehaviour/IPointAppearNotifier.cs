using Assets.MyFolder.Scripts.Basics;
using Unity.Collections;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public interface IPointAppearNotifier
    {
        void NextPoint(NativeArray<Translation> nextTranslations, NativeArray<Point> nextPoints);
    }
}
