using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UIComponent
{
    [RequireComponent(typeof(Button))]
    public class SelectionButton : MultiLayerButton
    {
        [Header("Name")]
        [SerializeField] private TextMeshProUGUI deckNameTextMesh;
        [SerializeField] private string deckName;
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
            deckName = name;
            deckNameTextMesh.text = name;
        }

        public void SetNameSpacing(float spacing)
        {
            nameSpacing = spacing;
            deckNameTextMesh.transform.localPosition = new Vector2(0, -(targetGraphic.rectTransform.sizeDelta.y / 2 + spacing));
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
            if (isSelected)
            {
                targetGraphic.CrossFadeColor(selectedColor * colors.colorMultiplier, 0f, true, true);

                foreach (ButtonLayer layer in buttonLayers)
                {
                    layer.image.CrossFadeColor(selectedColor * colors.colorMultiplier, 0f, true, true);
                }
            }
            else
            {
                base.DoStateTransition(state, instant);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            deckNameTextMesh.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetGraphic.rectTransform.sizeDelta.x);
            deckNameTextMesh.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetGraphic.rectTransform.sizeDelta.y * 0.75f);


            deckNameTextMesh.transform.localPosition = new Vector2(0, -(targetGraphic.rectTransform.sizeDelta.y / 2 + nameSpacing));
        }

        protected override void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            base.OnValidate();

            SetDeckName(deckName);
            SetNameSpacing(nameSpacing);
            SetCounter(count);
        }
    }
}