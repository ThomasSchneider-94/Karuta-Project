using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace Karuta.UIComponent
{
    public class DeckSelectionButton : ThreeLayerButton
    {
        [SerializeField] private TextMeshProUGUI buttonText;
        private string deckName;

        [SerializeField] private Mask mask;

        [SerializeField] private Button button;
        private Color iconBackgroundNonIteractableColor;
        private Color iconNonIteractableColor;

        // Called each time there is a modification in the script or editor (not used during runtime)
        override protected void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            base.OnValidate();

            SetDeckName(deckName);
            SetInteractable(button.interactable);
        }

        #region Deck Name
        public void SetDeckName(string name)
        {
            deckName = name;
            buttonText.text = name;
        }

        public string GetDeckName()
        {
            return deckName;
        }
        #endregion Deck Name

        #region Mask
        public override void SetIconScale(Vector2 scale)
        {
            mask.transform.localScale = scale;
        }
        #endregion Mask

        #region Button Interaction
        public override void SetIconBackgroundColor(Color color)
        {
            base.SetIconBackgroundColor(color);

            iconBackgroundNonIteractableColor.r = (float)NonInteractableRed(iconBackgroundColor.r);
            iconBackgroundNonIteractableColor.g = (float)NonInteractableGreen(iconBackgroundColor.g);
            iconBackgroundNonIteractableColor.b = (float)NonInteractableBlue(iconBackgroundColor.b);
            iconBackgroundNonIteractableColor.a = 1;
        }

        public override void SetIconColor(Color color)
        {
            base.SetIconColor(color);

            iconNonIteractableColor.r = (float)NonInteractableRed(iconColor.r);
            iconNonIteractableColor.g = (float)NonInteractableGreen(iconColor.g);
            iconNonIteractableColor.b = (float)NonInteractableGreen(iconColor.b);
            iconNonIteractableColor.a = 1;
        }

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;

            if (interactable)
            {
                iconBackground.color = iconBackgroundColor;
                icon.color = iconColor;
            }
            else
            {               
                iconBackground.color = iconBackgroundNonIteractableColor;
                icon.color = iconNonIteractableColor;
            }
        }

        #region Colors
        /*
         * To understand the choose for the magic numbers in this section, see the ColorExperiments document
         */

        // Gives an approximation of the non interactable red component of the color
        private double NonInteractableRed(float red)
        {
            return (0.5803922 - 0.08627451) * red + 0.08627451;
        }
        // Gives an approximation of the non interactable green component of the color
        private double NonInteractableGreen(float green)
        {
            return (0.5921569 - 0.1607843) * green + 0.1607843;
        }
        // Gives an approximation of the non interactable blue component of the color
        private double NonInteractableBlue(float blue)
        {
            return (0.6117647 - 0.2313726) * blue + 0.2313726;
        }
        #endregion Colors

        public bool GetInteractable()
        {
            return button.interactable;
        }
        #endregion Button Interaction
    }
}
