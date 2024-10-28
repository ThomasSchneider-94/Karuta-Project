using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UIComponent
{
    [System.Serializable]
    public class ButtonLayer
    {
        public Image image;
        public Sprite sprite;
        public Color color;
        public Vector2 scale;
    }

    public class MultiLayerButton : Button
    {
        [Header("Button Layers")]
        [SerializeField] protected List<ButtonLayer> buttonLayers;

        public void AddButtonLayer(ButtonLayer buttonLayer)
        {
            buttonLayers.Add(buttonLayer);
        }

        #region Setters
        public void SetImage(int i, Image image)
        {
            buttonLayers[i].image = image;
            buttonLayers[i].image.sprite = buttonLayers[i].sprite;
            buttonLayers[i].image.color = buttonLayers[i].color;
            buttonLayers[i].image.transform.localScale = buttonLayers[i].scale;
        }

        public void SetSprite(int i, Sprite sprite)
        {
            buttonLayers[i].sprite = sprite;
            buttonLayers[i].image.sprite = sprite;
        }

        /// <summary>
        /// Set the sprite of the icon. The Icon is the last layer in buttonLayers
        /// </summary>
        /// <param name="sprite"></param>
        public void SetIconSprite(Sprite sprite)
        {
            if (buttonLayers.Count > 0)
            {
                SetSprite(buttonLayers.Count - 1, sprite);
            }
            else
            {
                targetGraphic.GetComponent<Image>().sprite = sprite;
            }
        }

        public void SetColor(int i, Color color)
        {
            buttonLayers[i].color = color;
            buttonLayers[i].image.color = color;
        }

        public void SetScale(int i, Vector2 scale)
        {
            buttonLayers[i].scale = scale;
            buttonLayers[i].image.transform.localScale = scale;
        }
        #endregion Setters

        #region Getter
        public ButtonLayer GetButtonElement(int i)
        {
            return buttonLayers[i];
        }

        public Image GetImage(int i)
        {
            return buttonLayers[i].image;
        }

        public Sprite GetSprite(int i)
        {
            return buttonLayers[i].sprite;
        }

        public Color GetColor(int i)
        {
            return buttonLayers[i].color;
        }

        public Vector2 GetScale(int i)
        {
            return buttonLayers[i].scale;
        }
        #endregion Getter

        protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (transition == Transition.ColorTint)
            {
                foreach (ButtonLayer layer in buttonLayers)
                {
                    // Switch for a single variable
                    Color tintColor = state switch
                    {
                        SelectionState.Normal => colors.normalColor,
                        SelectionState.Highlighted => colors.highlightedColor,
                        SelectionState.Pressed => colors.pressedColor,
                        SelectionState.Selected => colors.selectedColor,
                        SelectionState.Disabled => colors.disabledColor,
                        _ => Color.black,
                    };
                    layer.image.CrossFadeColor(tintColor * colors.colorMultiplier, instant ? 0f : colors.fadeDuration, true, true);
                }

            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            Vector2 size = targetGraphic.rectTransform.sizeDelta;

            foreach (ButtonLayer buttonLayer in buttonLayers)
            {
                buttonLayer.image.rectTransform.sizeDelta = size;
            }
        }

        protected override void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            base.OnValidate();

            foreach (ButtonLayer buttonLayer in buttonLayers)
            {
                buttonLayer.image.sprite = buttonLayer.sprite;
                buttonLayer.image.color = buttonLayer.color;
                buttonLayer.image.transform.localScale = buttonLayer.scale;
            }
        }
    }
}