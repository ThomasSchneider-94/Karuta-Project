using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Karuta.UI.CustomButton
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
            if (i == 0)
            {
                targetGraphic = image;
            }
            else
            {
                buttonLayers[i - 1].image = image;
            }
        }

        public void SetSprite(int i, Sprite sprite)
        {
            if (i == 0)
            {
                image.sprite = sprite;
            }
            else
            {
                buttonLayers[i - 1].sprite = sprite;
                buttonLayers[i - 1].image.sprite = sprite;
            }
        }

        /// <summary>
        /// Set the texture of the icon. The Icon is the last layer in buttonLayers
        /// </summary>
        /// <param name="sprite"></param>
        public void SetIconSprite(Sprite sprite)
        {
            if (buttonLayers.Count > 0)
            {
                buttonLayers[^1].sprite = sprite;
                buttonLayers[^1].image.sprite = sprite;
            }
            else
            {
                targetGraphic.GetComponent<Image>().sprite = sprite;
            }
        }

        public void SetColor(int i, Color color)
        {
            if (i == 0)
            {
                image.color = color;
            }
            else
            {
                buttonLayers[i - 1].color = color;
                buttonLayers[i - 1].image.color = color;
            }
        }

        public void SetScale(int i, Vector2 scale)
        {
            if (i == 0)
            {
                transform.localScale = scale;
            }
            else
            {
                buttonLayers[i - 1].scale = scale;
                buttonLayers[i - 1].image.transform.localScale = scale;
            }
        }
        #endregion Setters

        #region Getter
        public ButtonLayer GetButtonLayer(int i)
        {
            if (i == 0)
            {
                return new()
                {
                    image = image,
                    sprite = image.sprite,
                    color = image.color,
                    scale = transform.localScale
                };
            }
            return buttonLayers[i - 1];
        }

        public Image GetImage(int i)
        {
            if (i == 0)
            {
                return image;
            }
            return buttonLayers[i - 1].image;
        }

        public Sprite GetSprite(int i)
        {
            if (i == 0)
            {
                return image.sprite;
            }
            return buttonLayers[i - 1].sprite;
        }

        public Color GetColor(int i)
        {
            if (i == 0)
            {
                return image.color;
            }
            return buttonLayers[i - 1].color;
        }

        public Vector2 GetScale(int i)
        {
            if (i == 0)
            {
                return transform.localScale;
            }
            return buttonLayers[i - 1].scale;
        }

        public List<Image> GetImages()
        {
            List<Image> images = new() { image };

            foreach (ButtonLayer layer in buttonLayers)
            {
                images.Add(layer.image);
            }

            return images;
        }
        #endregion Getter

        protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (transition == Transition.ColorTint)
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
                foreach (ButtonLayer layer in buttonLayers)
                {
                    layer.image.CrossFadeColor(tintColor * colors.colorMultiplier, instant ? 0f : colors.fadeDuration, true, true);
                }
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            foreach (ButtonLayer buttonLayer in buttonLayers)
            {
                buttonLayer.image.rectTransform.sizeDelta = targetGraphic.rectTransform.sizeDelta;
            }
        }

#if UNITY_EDITOR
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
#endif
    }
}