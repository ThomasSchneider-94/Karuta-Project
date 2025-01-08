using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace Karuta.UIComponent
{
    public class SelectionButton : MultiLayerButton
    {
        [Header("Name")]
        [SerializeField] private TextMeshProUGUI deckNameTextMesh;
        [SerializeField] private string deckName;
        [SerializeField] private float nameWidth;
        [SerializeField] private float nameSpacing = 20;

        [Header("Selected Color")]
        [SerializeField] private bool isSelected = false;
        [SerializeField] private Color selectedColor;

        [Header("Counter")]
        [SerializeField] private int count;
        [SerializeField] private Image counterImage;
        [SerializeField] private TextMeshProUGUI counterTextMesh;

        #region Setter
        public void SetDeckName(string name)
        {
            this.deckName = name;
            deckNameTextMesh.text = name;
        }

        public void SetNameWidth(float width)
        {
            this.nameWidth = width;
            deckNameTextMesh.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public void SetNameSpacing(float spacing)
        {
            this.nameSpacing = spacing;
            deckNameTextMesh.transform.localPosition = new Vector2(0, -(targetGraphic.rectTransform.sizeDelta.y / 2 + spacing));
        }

        public void SetSelectedColor(Color selectedColor)
        {
            this.selectedColor = selectedColor;
        }
        #endregion Setter

        #region Getter
        public string GetDeckName()
        {
            return deckName;
        }

        public float GetNameSpacing()
        {
            return nameSpacing;
        }
        #endregion Getter

        #region Selection
        public void SelectButton()
        {
            isSelected = true;
            targetGraphic.CrossFadeColor(selectedColor * colors.colorMultiplier, 0f, true, true);

            // Counter
            count++;
            counterImage.gameObject.SetActive(count > 1);
            counterTextMesh.text = count.ToString();

            foreach (ButtonLayer layer in buttonLayers)
            {
                layer.image.CrossFadeColor(selectedColor * colors.colorMultiplier, 0f, true, true);
            }
        }

        public void DeselectButton()
        {
            isSelected = false;
            targetGraphic.CrossFadeColor(colors.normalColor * colors.colorMultiplier, 0f, true, true);

            // Counter
            count = 0;
            counterImage.gameObject.SetActive(false);

            foreach (ButtonLayer layer in buttonLayers)
            {
                layer.image.CrossFadeColor(colors.normalColor * colors.colorMultiplier, 0f, true, true);
            }
        }

        public void SetCounter(int count)
        {
            this.count = count;
            counterImage.gameObject.SetActive(count > 1);
            counterTextMesh.text = count.ToString();
        }
        #endregion Selection

        protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            // Switch for a single variable
            Color tintColor = isSelected switch
            {
                true => selectedColor,
                false => colors.normalColor,
            };

            targetGraphic.CrossFadeColor(tintColor * colors.colorMultiplier, 0f, true, true);

            foreach (ButtonLayer layer in buttonLayers)
            {
                layer.image.CrossFadeColor(tintColor * colors.colorMultiplier, 0f, true, true);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            if (deckNameTextMesh != null)
            {
                deckNameTextMesh.transform.localPosition = new Vector2(0, -(targetGraphic.rectTransform.sizeDelta.y / 2 + nameSpacing));
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            base.OnValidate();

            if (deckNameTextMesh != null)
            {
                SetDeckName(deckName);
                SetNameWidth(nameWidth);
                SetNameSpacing(nameSpacing);
                
            }
            if (counterTextMesh != null)
            {
                SetCounter(count);
            }
        }
#endif
    }
}