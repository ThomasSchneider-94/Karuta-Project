using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UIComponent
{
    [RequireComponent(typeof(Button))]
    public class SelectionButton : ThreeLayerButton
    {
        [Header("Name")]
        [SerializeField] private TextMeshProUGUI deckNameTextMesh;
        [SerializeField] private string deckName;
        [SerializeField] private float nameSpacing = 20;

        [Header("Mask")]
        [SerializeField] private Image mask;

        [Header("Button")]
        [SerializeField] private Button button;
        [SerializeField] private bool isInteractable;
        [SerializeField] private bool isSelectionned;

        #region Deck Name
        public void SetDeckName(string name)
        {
            deckName = name;
            deckNameTextMesh.text = name;
        }

        public void SetNameSpacing(float spacing)
        {
            nameSpacing = spacing;
            deckNameTextMesh.transform.localPosition = new Vector2(0, -(thirdLayerImage.rectTransform.sizeDelta.y / 2 + spacing));
        }

        public string GetDeckName()
        {
            return deckName;
        }

        public float GetNameSpacing()
        {
            return nameSpacing;
        }
        #endregion Deck Name

        #region Mask
        public override void SetSecondLayerSprite(Sprite sprite)
        {
            base.SetSecondLayerSprite(sprite);
            mask.sprite = sprite;
        }

        public override void SetThirdLayerScale(Vector2 scale)
        {
            secondLayerScale = scale;
            mask.transform.localScale = scale;
        }
        #endregion Mask

        #region Button Interaction

        #region Colors
        private Color CalculateSecondLayerNonIteractableColor()
        {
            return new Color((float)NonInteractableRed(secondLayerColor.r),
                             (float)NonInteractableGreen(secondLayerColor.g),
                             (float)NonInteractableBlue(secondLayerColor.b),
                             1);
        }

        private Color CalculateThirdLayerNonIteractableColor()
        {
            return new Color((float)NonInteractableRed(thirdLayerColor.r),
                             (float)NonInteractableGreen(thirdLayerColor.g),
                             (float)NonInteractableBlue(thirdLayerColor.b),
                             1);
        }

        /*
         * To understand the choose for the magic numbers in this section, see the ColorExperiments document
         */

        /// <summary>
        /// Gives an approximation of the non interactable red component of the color
        /// </summary>
        /// <param name="red"></param>
        /// <returns></returns>
        private static double NonInteractableRed(float red)
        {
            return (0.5803922 - 0.08627451) * red + 0.08627451;
        }

        /// <summary>
        /// Gives an approximation of the non interactable green component of the color
        /// </summary>
        /// <param name="green"></param>
        /// <returns></returns>
        private static double NonInteractableGreen(float green)
        {
            return (0.5921569 - 0.1607843) * green + 0.1607843;
        }

        /// <summary>
        /// Gives an approximation of the non interactable blue component of the color
        /// </summary>
        /// <param name="blue"></param>
        /// <returns></returns>
        private static double NonInteractableBlue(float blue)
        {
            return (0.6117647 - 0.2313726) * blue + 0.2313726;
        }
        #endregion Colors

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;

            if (interactable)
            {
                secondLayerImage.color = secondLayerColor;
                thirdLayerImage.color = thirdLayerColor;
            }
            else
            {
                secondLayerImage.color = CalculateSecondLayerNonIteractableColor();
                thirdLayerImage.color = CalculateThirdLayerNonIteractableColor();
            }
        }

        public bool GetInteractable()
        {
            return button.interactable;
        }
        #endregion Button Interaction

        #region Button Selection
        public void SetSelectionned(bool selectionned)
        {
            isSelectionned = selectionned;
            Debug.Log(deckName + " " + selectionned);
        }

        public bool GetSelectionned()
        {
            return isSelectionned;
        }
        #endregion Button Selection

        #region Button Function





        #endregion Button Function

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            Vector2 size = firstLayerImage.rectTransform.sizeDelta;
            
            mask.rectTransform.sizeDelta = size;
            deckNameTextMesh.rectTransform.sizeDelta = Vector2.Scale(size, new Vector2(1.5f, 0.25f));
            deckNameTextMesh.rectTransform.localPosition = new Vector2(0, -(size.y / 2 + nameSpacing));
        }
        
        protected override void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            base.OnValidate();

            SetDeckName(deckName);
            SetNameSpacing(nameSpacing);
            SetInteractable(isInteractable);
        }
    }
}