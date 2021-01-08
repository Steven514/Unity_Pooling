using UnityEngine;
using System.Collections.Generic;

namespace Valklabs.Examples.Pooling
{
    //An example class that the Unity Events on the monopoolercontroller hook into on the scene to show a demo how to use the Unity Events. Each UI object represents a bullet instance and it's pooled state
    public class ExampleAmmoContainerUI : MonoBehaviour
    {
        [SerializeField] private ExampleAmmoItemUI _ammoItemUIPrefab;

        private IList<ExampleAmmoItemUI> _ammoItems = new List<ExampleAmmoItemUI>();
        private int _ammoIndex = 0;

        private float _isActiveAlpha = 0.5f;
        private float _isInactiveAlpha = 1f;

        public void OnBulletInstanceCreated()
        {
            if(_ammoItemUIPrefab == null)
            {
                return;
            }

            //Create a UI object to represent the newly created ammo
            ExampleAmmoItemUI ammoItem = Instantiate(_ammoItemUIPrefab, transform);
            ammoItem.SetAlpha(_isInactiveAlpha);
            _ammoItems?.Add(ammoItem);
        }

        public void OnBulletInstanceSpawned()
        {
            if(_ammoItems == null)
            {
                return;
            }

            _ammoItems[_ammoIndex]?.SetAlpha(_isActiveAlpha);
            _ammoIndex++;
        }

        public void OnBulletInstanceDespawned()
        {
            if (_ammoItems == null)
            {
                return;
            }
            _ammoIndex--;
            _ammoItems[_ammoIndex]?.SetAlpha(_isInactiveAlpha);
        }
    }
}
