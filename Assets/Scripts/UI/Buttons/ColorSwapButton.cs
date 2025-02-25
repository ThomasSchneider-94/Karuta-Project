using UnityEditor;
using UnityEngine;

namespace Karuta.UI.CustomButton
{
    public class ColorSwapButton : SelectableButton
    {
        [SerializeField] private Color baseColor;
        public Color BaseColor => baseColor;

        override protected void SelectButton()
        {
            base.SelectButton();

            targetGraphic.color = SelectedColor;
        }

        override protected void DeselectButton()
        {
            base.DeselectButton();

            targetGraphic.color = baseColor;
        }

        override public void SetSelectedColor(Color color)
        {
            base.SetSelectedColor(color);

            targetGraphic.color = isSelected ? SelectedColor : BaseColor;
        }

        public void SetBaseColor(Color color)
        {
            baseColor = color;
            targetGraphic.color = isSelected ? SelectedColor : BaseColor;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            base.OnValidate();

            SetSelectedColor(SelectedColor);
            SetBaseColor(BaseColor);
            SelectButton(isSelected);
        }
#endif
    }
}