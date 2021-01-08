using UnityEngine;

namespace Valklabs.Util.Pooling
{
    public interface IPoolController<T>
    {
        T Get();

        T Get(Vector3 spawnAt);

        T Get(Vector3 spawnAt, Quaternion rotation);

        T Get(Transform spawnAtTransform, bool isWorldPositionStays);

        T Get(Transform spawnAtTransform, bool isWorldPositionStays, Quaternion rotation);

        void OnCleanUp();
    }

}
