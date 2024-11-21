using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UIComponent
{
    public class NumberButton : Button
    {
        [SerializeField] private Color baseColor;

        public void SelectButton()
        {
            targetGraphic.color = colors.selectedColor;
        }

        public void DeselectButton()
        {
            targetGraphic.color = baseColor;
        }
    }
}