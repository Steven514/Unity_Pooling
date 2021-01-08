using System;

namespace Valklabs.Util.Pooling
{
    public interface IPoolable
    {
        /// <summary>
        /// Ever only called once when/if an instance is created by the pooler.
        /// </summary>
        void OnCreate();

        /// <summary>
        /// This is where you may want to set your gameobject active, or anything else depending on your pooling needs. 
        /// </summary>
        void OnSpawn(Action<IPoolable> onReturnToPool);

        /// <summary>
        /// This is where you may want to set your gameobject inactive, or anything else depending on your pooling needs. 
        /// You'll need to cache the passed action in 'onReturnToPool' action from OnSpawn in the OnDespawn. 
        /// </summary>
        void OnDespawn();


    }

}
