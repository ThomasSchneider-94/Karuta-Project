using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using Karuta.UIComponent;
using UnityEngine.UI;
using Karuta.Objects;


namespace Karuta.Commons
{
    public abstract class ThemeApplier : MonoBehaviour
    {
        protected ThemeManager themeManager;
        
        [Header("Background")]
        [SerializeField] protected RawImage backgroundRawImage;
        [SerializeField] protected VideoPlayer videoPlayer;
        [SerializeField] protected RenderTexture renderTexture;

        [Header("Arrow Buttons")]
        [SerializeField] private List<MultiLayerButton> arrowButtons;
        [SerializeField] private List<MultiLayerButton> reverseArrowButtons;

        #region Options
        [Header("Option Buttons")]
        [SerializeField] private List<MultiLayerButton> optionButtons;

        [Header("Option Toggles")]
        [SerializeField] private List<LabeledToggle> optionToggles;

        [Header("Option Panels")]
        [SerializeField] private List<Image> optionPanelsBorders;
        [SerializeField] private List<Image> optionPanels;

        [Header("Close Buttons")]
        [SerializeField] private List<MultiLayerButton> closeButtons;
        #endregion Options

        virtual protected void OnEnable()
        {
            themeManager = ThemeManager.Instance;

            // Initialize
            if (themeManager.IsInitialized())
            {
                ApplyTheme();
            }
            else
            {
                themeManager.InitializeThemeEvent.AddListener(ApplyTheme);
            }




        }


















        #region Theme Applier
        public void ApplyTheme()
        {
            ApplyTheme(themeManager.GetCurrentTheme(), themeManager.GetBaseTheme());
        }

        protected virtual void ApplyTheme(Theme currentTheme, BaseTheme baseTheme)
        {
            ApplyBackground(currentTheme, baseTheme);

            ApplyButtonsColors(currentTheme, baseTheme);

            ApplyTogglesColors(currentTheme, baseTheme);

            ApplyOptionPanelColors(currentTheme, baseTheme);
        }

        protected abstract void ApplyBackground(Theme currentTheme, BaseTheme baseTheme);

        virtual protected void ApplyButtonsColors(Theme currentTheme, BaseTheme baseTheme)
        {
            // Option Buttons
            foreach (MultiLayerButton button in optionButtons)
            {
                button.SetColor(0, GetColorFromString(currentTheme.optionButtonOutlineColor, baseTheme.optionButtonOutlineColor));
                button.SetColor(1, GetColorFromString(currentTheme.optionButtonInsideColor, baseTheme.optionButtonInsideColor));
                button.SetColor(2, GetColorFromString(currentTheme.optionButtonIconColor, baseTheme.optionButtonIconColor));
            }

            // Close Buttons
            foreach (MultiLayerButton button in closeButtons)
            {
                button.SetColor(0, GetColorFromString(currentTheme.closeButtonOutlineColor, baseTheme.closeButtonOutlineColor));
                button.SetColor(1, GetColorFromString(currentTheme.closeButtonInsideColor, baseTheme.closeButtonInsideColor));
                button.SetColor(2, GetColorFromString(currentTheme.closeButtonIconColor, baseTheme.closeButtonIconColor));
            }

            // Arrow Buttons
            foreach (MultiLayerButton button in arrowButtons)
            {
                button.SetColor(0, GetColorFromString(currentTheme.arrowButtonOutsideColor, baseTheme.arrowButtonOutsideColor));
                button.SetColor(1, GetColorFromString(currentTheme.arrowButtonInsideColor, baseTheme.arrowButtonInsideColor));
                button.GetLabel().color = GetColorFromString(currentTheme.arrowButtonTextColor, baseTheme.arrowButtonTextColor);
            }

            // Reverse Arrow Buttons
            foreach (MultiLayerButton button in reverseArrowButtons)
            {
                button.SetColor(0, GetColorFromString(currentTheme.reverseArrowButtonOutsideColor, baseTheme.reverseArrowButtonOutsideColor));
                button.SetColor(1, GetColorFromString(currentTheme.reverseArrowButtonInsideColor, baseTheme.reverseArrowButtonInsideColor));
                button.GetLabel().color = GetColorFromString(currentTheme.reverseArrowButtonTextColor, baseTheme.reverseArrowButtonTextColor);
            }
        }

        virtual protected void ApplyTogglesColors(Theme currentTheme, BaseTheme baseTheme)
        {
            // Option Toggles
            foreach (LabeledToggle toggle in optionToggles)
            {
                toggle.GetText().color = GetColorFromString(currentTheme.optionsTogglesLabelColor, baseTheme.optionsTogglesLabelColor);
                toggle.GetOutline().effectColor = GetColorFromString(currentTheme.optionsTogglesLabelOutlineColor, baseTheme.optionsTogglesLabelOutlineColor);
                toggle.GetBackgoundOutline().color = GetColorFromString(currentTheme.optionsTogglesCheckBoxOutlineColor, baseTheme.optionsTogglesCheckBoxOutlineColor);
                toggle.GetBackgound().color = GetColorFromString(currentTheme.optionsTogglesCheckBoxColor, baseTheme.optionsTogglesCheckBoxColor);
                toggle.graphic.color = GetColorFromString(currentTheme.optionsTogglesCheckMarkColor, baseTheme.optionsTogglesCheckMarkColor);
            }
        }

        protected void ApplyOptionPanelColors(Theme currentTheme, BaseTheme baseTheme)
        {
            foreach (Image image in optionPanelsBorders)
            {
                image.color = GetColorFromString(currentTheme.optionPanelBorderColor, baseTheme.optionPanelBorderColor);
            }
            foreach (Image image in optionPanels)
            {
                image.color = GetColorFromString(currentTheme.optionPanelColor, baseTheme.optionPanelColor);
            }
        }
        #endregion Theme Applier

        #region From string getter
        protected static Background GetBackgroundFromString(string backgroundString, Background defaultBackground)
        {
            if (string.IsNullOrEmpty(backgroundString))
            {
                return defaultBackground;
            }
            bool isTexture = backgroundString.Split(".")[^1] == "png";

            return new()
            {
                isTexture = isTexture,
                texture = isTexture ? LoadManager.LoadThemeTexture(backgroundString) : null,
                videoPath = isTexture ? null : backgroundString,
            };
        }
        
        protected static Color GetColorFromString(string colorString, Color defaultColor)
        {
            if (ColorUtility.TryParseHtmlString(colorString, out Color color))
            {
                return color;
            }
            return defaultColor;
        }
        #endregion From string getter






#if UNITY_EDITOR
        public virtual void VisualizeApplication(Theme currentTheme, BaseTheme baseTheme)
        {
            ApplyTheme(currentTheme, baseTheme);
        }
#endif
    }
}