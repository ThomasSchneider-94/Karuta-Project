using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UIComponent
{
    public class NumberButton : Button
    {
        [SerializeField] private Color baseColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private TextMeshProUGUI text;

        public void SelectButton()
        {
            targetGraphic.color = selectedColor;
        }

        public void DeselectButton()
        {
            targetGraphic.color = baseColor;
        }

        public void SetBaseColor(Color color)
        {
            this.baseColor = color;
        }

        public void SetSelectionColor(Color color)
        {
            this.selectedColor = color;
        }

        public TextMeshProUGUI GetText()
        {
            return this.text;
        }
    }
}