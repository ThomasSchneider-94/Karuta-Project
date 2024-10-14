using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UIComponent
{
    public class ThreeLayerButton : MonoBehaviour
    {
        [Header("First Layer Values")]
        [SerializeField] protected Color firstLayerColor;
        [SerializeField] protected Sprite firstLayerSprite;
        [SerializeField] protected Image firstLayerImage;

        [Header("Second Layer Values")]
        [SerializeField] protected Color secondLayerColor;
        [SerializeField] protected Sprite secondLayerSprite;
        [SerializeField] protected Vector2 secondLayerScale;
        [SerializeField] protected Image secondLayerImage;

        [Header("Third Layer Values")]
        [SerializeField] protected Color thirdLayerColor;
        [SerializeField] protected Sprite thirdLayerSprite;
        [SerializeField] protected Vector2 thirdLayerScale;
        [SerializeField] protected Image thirdLayerImage;

        #region First Layer
        public virtual void SetFirstLayerColor(Color color)
        {
            firstLayerColor = color;
            firstLayerImage.color = color;
        }

        public virtual void SetFirstLayerSprite(Sprite sprite)
        {
            firstLayerSprite = sprite;
            firstLayerImage.sprite = sprite;
        }

        public Color GetFirstLayerColor()
        {
            return firstLayerColor;
        }

        public Sprite GetFirstLayerSprite()
        {
            return firstLayerSprite;
        }
        #endregion First Layer

        #region Second Layer
        public virtual void SetSecondLayerColor(Color color)
        {
            secondLayerColor = color;
            secondLayerImage.color = color;
        }

        public virtual void SetSecondLayerSprite(Sprite sprite)
        {
            secondLayerSprite = sprite;
            secondLayerImage.sprite = sprite;
        }

        public virtual void SetSecondLayerScale(Vector2 scale)
        {
            secondLayerScale = scale;
            secondLayerImage.transform.localScale = scale;
        }

        public Color GetSecondLayerColor()
        {
            return secondLayerColor;
        }

        public Sprite GetSecondLayerSprite()
        {
            return secondLayerSprite;
        }

        public Vector2 GetSecondLayerScale()
        {
            return secondLayerScale;
        }
        #endregion Second Layer

        #region Third Layer
        public virtual void SetThirdLayerColor(Color color)
        {
            thirdLayerColor = color;
            thirdLayerImage.color = color;
        }

        public virtual void SetThirdLayerSprite(Sprite sprite)
        {
            thirdLayerSprite = sprite;
            thirdLayerImage.sprite = sprite;
        }

        public virtual void SetThirdLayerScale(Vector2 scale)
        {
            thirdLayerScale = scale;
            thirdLayerImage.transform.localScale = scale;
        }

        public Color GetThirdLayerColor()
        {
            return thirdLayerColor;
        }

        public Sprite GetThirdLayerSprite()
        {
            return thirdLayerSprite;
        }

        public Vector2 GetThirdLayerScale()
        {
            return thirdLayerScale;
        }
        #endregion Third Layer

        protected virtual void OnRectTransformDimensionsChange()
        {
            Vector2 size = firstLayerImage.rectTransform.sizeDelta;

            secondLayerImage.rectTransform.sizeDelta = size;
            thirdLayerImage.rectTransform.sizeDelta = size;
        }

        protected virtual void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            SetFirstLayerColor(firstLayerColor);
            SetFirstLayerSprite(firstLayerSprite);

            SetSecondLayerColor(secondLayerColor);
            SetSecondLayerSprite(secondLayerSprite);
            SetSecondLayerScale(secondLayerScale);

            SetThirdLayerColor(thirdLayerColor);
            SetThirdLayerSprite(thirdLayerSprite);
            SetThirdLayerScale(thirdLayerScale);
        }
    }
}