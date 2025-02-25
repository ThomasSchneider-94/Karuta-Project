using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UI.CustomButton
{
    public class DeckButton : ColorFadeButton
    {
        [Header("General")]
        [SerializeField] private string deckName;
        [SerializeField] private float nameWidth;
        [SerializeField] private float nameSpacing;
        [SerializeField] private int count;
        public string DeckName => deckName;
        public float NameWidth => nameWidth;
        public float NameSpacing => nameSpacing;
        public int Count => count;

        private Image counterImage;
        private TextMeshProUGUI deckNameTextMesh;
        private TextMeshProUGUI counterTextMesh;

        override protected void Awake()
        {
            base.Awake();

            counterImage = transform.Find("Counter").GetComponent<Image>();
            counterTextMesh = counterImage.GetComponentInChildren<TextMeshProUGUI>();
            deckNameTextMesh = transform.Find("Name").GetComponentInChildren<TextMeshProUGUI>();
        }

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

        public void SetCounter(int count)
        {
            this.count = count;
            counterImage.gameObject.SetActive(count > 1);
            counterTextMesh.text = count.ToString();
        }
        #endregion Setter

        #region Selection
        override protected void SelectButton()
        {
            // Counter
            count++;
            counterImage.gameObject.SetActive(Count > 1);
            counterTextMesh.text = Count.ToString();

            base.SelectButton();
        }

        override protected void DeselectButton()
        {
            // Counter
            count = 0;
            counterImage.gameObject.SetActive(false);

            base.DeselectButton();
        }
        #endregion Selection

        protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
        {
            if (!interactable)
            {
                targetGraphic.CrossFadeColor(colors.disabledColor * colors.colorMultiplier, 0f, true, true);

                foreach (ButtonLayer layer in buttonLayers)
                {
                    layer.image.CrossFadeColor(colors.disabledColor * colors.colorMultiplier, 0f, true, true);
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

            if (deckNameTextMesh == null)
            {
                deckNameTextMesh = transform.Find("Name").GetComponentInChildren<TextMeshProUGUI>();
            }
            deckNameTextMesh.transform.localPosition = new Vector2(0, -(targetGraphic.rectTransform.sizeDelta.y / 2 + NameSpacing));
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            base.OnValidate();

            Awake();

            SetDeckName(DeckName);
            SetNameWidth(NameWidth);
            SetNameSpacing(NameSpacing);

            SetCounter(Count);
        }
#endif
    }
}