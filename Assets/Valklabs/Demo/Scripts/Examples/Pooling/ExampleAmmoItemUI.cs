using UnityEngine;
using UnityEngine.UI;

namespace Valklabs.Examples.Pooling
{
    public class ExampleAmmoItemUI : MonoBehaviour
    {
        [SerializeField] private Image _ammoImage;

        public void SetAlpha(float alpha)
        {
            if(_ammoImage == null)
            {
                return;
            }

            //Get the current color
            Color color = _ammoImage.color;
            //set the current color's alpha
            color.a = alpha;

            //apply color with the desired alpha. We'll use this to represent if a bullet is active (no fade) or inactive (faded)
            _ammoImage.color = color;
        }
    }
}
