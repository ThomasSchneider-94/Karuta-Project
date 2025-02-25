using UnityEditor;
using UnityEngine;

namespace Karuta.UI.CustomButton
{
    public abstract class SelectableButton : MultiLayerButton
    {
        [Header("Selection")]
        [SerializeField] protected bool isSelected;
        [SerializeField] private Color selectedColor;
        public Color SelectedColor => selectedColor;

        public void SelectButton(bool isSelected)
        {
            if (isSelected)
            {
                SelectButton();
            }
            else
            {
                DeselectButton();
            }
        }

        public void SwitchSelection()
        {
            SelectButton(!isSelected);
        }

        virtual protected void SelectButton()
        {
            isSelected = true;
        }

        virtual protected void DeselectButton()
        {
            isSelected = false;
        }

        virtual public void SetSelectedColor(Color color)
        {
            selectedColor = color;
        }

#if UNITY_EDITOR
        // Called if changes in code or editor (not called at runtime)
        override protected void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            base.OnValidate();

            if (isSelected)
            {
                SelectButton();
            }
            else
            {
                DeselectButton();
            }
        }
#endif
    }
}