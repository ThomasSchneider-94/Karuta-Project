using UnityEngine.UI;
using UnityEngine;

namespace Karuta.UI.CustomButton
{
    public class ColorFadeButton : SelectableButton
    {
        override protected void SelectButton()
        {
            base.SelectButton();

            targetGraphic.CrossFadeColor(SelectedColor * colors.colorMultiplier, 0f, true, true);

            foreach (ButtonLayer layer in buttonLayers)
            {
                layer.image.CrossFadeColor(SelectedColor * colors.colorMultiplier, 0f, true, true);
            }
        }

        override protected void DeselectButton()
        {
            base.DeselectButton();

            targetGraphic.CrossFadeColor(colors.normalColor * colors.colorMultiplier, 0f, true, true);

            foreach (ButtonLayer layer in buttonLayers)
            {
                layer.image.CrossFadeColor(colors.normalColor * colors.colorMultiplier, 0f, true, true);
            }
        }

        override public void SetSelectedColor(Color color)
        {
            base.SetSelectedColor(color);

            Color tint = isSelected ? SelectedColor : colors.normalColor;

            targetGraphic.CrossFadeColor(tint * colors.colorMultiplier, 0f, true, true);

            foreach (ButtonLayer layer in buttonLayers)
            {
                layer.image.CrossFadeColor(tint * colors.colorMultiplier, 0f, true, true);
            }
        }


        // TODO : Maybe inutile car en mode animation
        override protected void DoStateTransition(Selectable.SelectionState state, bool instant)
        {
            if (!isSelected)
            {
                base.DoStateTransition(state, instant);
            }
        }
    }
}