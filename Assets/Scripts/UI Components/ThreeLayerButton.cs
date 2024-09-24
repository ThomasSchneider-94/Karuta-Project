using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta.UIComponent
{
    public class ThreeLayerButton : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] protected Sprite buttonImageSprite;
        [SerializeField] protected Sprite iconBackgoundSprite;
        [SerializeField] protected Sprite iconSprite;

        [Header("Colors")]
        [SerializeField] protected Color buttonImageColor;
        [SerializeField] protected Color iconBackgroundColor;
        [SerializeField] protected Color iconColor;

        [Header("Scales")]
        [SerializeField] protected Vector2 iconBackgroundScale;
        [SerializeField] protected Vector2 iconScale;

        [Header("Internal Object")]
        [SerializeField] protected Image buttonImage;
        [SerializeField] protected Image iconBackground;
        [SerializeField] protected Image icon;

        // Called each time there is a modification in the script or editor (not used during runtime)
        virtual protected void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            SetButtonSprite(buttonImageSprite);
            SetButtonColor(buttonImageColor);

            SetIconBackgroundSprite(iconBackgoundSprite);
            SetIconBackgroundScale(iconBackgroundScale);
            SetIconBackgroundColor(iconBackgroundColor);

            SetIconSprite(iconSprite);
            SetIconScale(iconScale);
            SetIconColor(iconColor);
        }

        #region Setters

        #region Sprites
        virtual public void SetButtonSprite(Sprite sprite)
        {
            buttonImageSprite = sprite;
            buttonImage.sprite = sprite;
        }
        virtual public void SetIconBackgroundSprite(Sprite sprite)
        {
            iconBackgoundSprite = sprite;
            iconBackground.sprite = sprite;
        }
        virtual public void SetIconSprite(Sprite sprite)
        {
            iconSprite = sprite;
            icon.sprite = sprite;
        }
        #endregion Sprites

        #region Colors
        virtual public void SetButtonColor(Color color)
        {
            buttonImageColor = color;
            buttonImage.color = color;
        }
        virtual public void SetIconBackgroundColor(Color color)
        {
            iconBackgroundColor = color;
            iconBackground.color = color;
        }
        virtual public void SetIconColor(Color color)
        {
            iconColor = color;
            icon.color = color;
        }
        #endregion Colors

        #region Scales

        virtual public void SetIconBackgroundScale(Vector2 scale)
        {
            iconBackgroundScale = scale;
            iconBackground.transform.localScale = scale;
        }
        virtual public void SetIconScale(Vector2 scale)
        {
            iconScale = scale;
            icon.transform.localScale = scale;
        }
        #endregion Scales

        #endregion Setters

        #region Getters

        #region Sprites
        public Sprite GetButtonSprite()
        {
            return buttonImageSprite;
        }
        public Sprite GetIconBackgroundSprite()
        {
            return iconBackgoundSprite;
        }
        public Sprite GetIconSprite()
        {
            return iconSprite;
        }
        #endregion Sprites

        #region Colors
        public Color GetButtonColor()
        {
            return buttonImageColor;
        }
        public Color GetIconBackgroundColor()
        {
            return iconBackgroundColor;
        }
        public Color GetIconColor()
        {
            return iconColor;
        }
        #endregion Colors

        #region Scales
        public Vector2 GetIconBackgroundScale()
        {
            return iconBackgroundScale;
        }
        public Vector2 GetIconScale()
        {
            return iconScale;
        }
        #endregion Scales

        #endregion Getters
    }
}