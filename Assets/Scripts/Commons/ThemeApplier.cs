using UnityEngine;
using UnityEngine.UI;
using Karuta.Objects;
using UnityEngine.Video;
using System.Net;
using System;
using Karuta.UIComponent;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.UI;


namespace Karuta.Commons
{
    public abstract class ThemeApplier : MonoBehaviour
    {
        protected ThemeManager themeManager;
        
        [Header("Background")]
        [SerializeField] protected RawImage backgroundRawImage;
        [SerializeField] protected VideoPlayer videoPlayer;
        [SerializeField] protected RenderTexture renderTexture;

        [Header("Buttons")]
        [SerializeField] private List<MultiLayerButton> optionButtons;
        [SerializeField] private List<MultiLayerButton> closeButtons;
        [SerializeField] private List<MultiLayerButton> arrowButtons;
        [SerializeField] private List<MultiLayerButton> reverseArrowButtons;

        [Header("Toggles")]
        [SerializeField] private List<LabeledToggle> optionToggles;

        [Header("Option Panels")]
        [SerializeField] private List<Image> optionPanelsBorders;
        [SerializeField] private List<Image> optionPanels;



        protected int previousTheme;

        virtual protected void OnEnable()
        {
            themeManager = ThemeManager.Instance;

            themeManager.UpdateThemeEvent.AddListener(SoftApplyTheme);

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









        public virtual void ApplyTheme()
        {
            ApplyBackground();

            ApplyButtonsColors();

            ApplyTogglesColors();

            ApplyOptionPanelToggle();
        }

        protected abstract void ApplyBackground();

        virtual protected void ApplyButtonsColors()
        {
            // Option Buttons
            foreach (MultiLayerButton button in optionButtons)
            {
                button.SetColor(0, themeManager.GetOptionButtonOutlineColor());
                button.SetColor(1, themeManager.GetOptionButtonInsideColor());
                button.SetColor(2, themeManager.GetOptionButtonIconColor());
            }

            // Close Buttons
            foreach (MultiLayerButton button in closeButtons)
            {
                button.SetColor(0, themeManager.GetCloseButtonOutlineColor());
                button.SetColor(1, themeManager.GetCloseButtonInsideColor());
                button.SetColor(2, themeManager.GetCloseButtonIconColor());
            }

            // Arrow Buttons
            foreach (MultiLayerButton button in arrowButtons)
            {
                button.SetColor(0, themeManager.GetArrowButtonOutsideColor());
                button.SetColor(1, themeManager.GetArrowButtonInsideColor());
                button.GetLabel().color = themeManager.GetArrowButtonTextColor();
            }

            // Reverse Arrow Buttons
            foreach (MultiLayerButton button in reverseArrowButtons)
            {
                button.SetColor(0, themeManager.GetReverseArrowButtonOutsideColor());
                button.SetColor(1, themeManager.GetReverseArrowButtonInsideColor());
                button.GetLabel().color = themeManager.GetReverseArrowButtonTextColor();
            }
        }

        virtual protected void ApplyTogglesColors()
        {
            // Option Toggles
            foreach (LabeledToggle toggle in optionToggles)
            {
                toggle.GetText().color = themeManager.GetOptionsTogglesLabelColor();
                toggle.GetOutline().effectColor = themeManager.GetOptionsTogglesLabelOutlineColor();
                toggle.GetBackgoundOutline().color = themeManager.GetOptionsTogglesCheckBoxOutlineColor();
                toggle.GetBackgound().color = themeManager.GetOptionsTogglesCheckBoxColor();
                toggle.graphic.color = themeManager.GetOptionsTogglesCheckMarkColor();
            }
        }

        protected void ApplyOptionPanelToggle()
        {
            foreach (Image image in optionPanelsBorders)
            {
                image.color = themeManager.GetOptionPanelBorderColor();
            }
            foreach (Image image in optionPanels)
            {
                image.color = themeManager.GetOptionPanelColor();
            }
        }









#if UNITY_EDITOR
        public virtual void VisualizeApplication(ThemeManager themeManager)
        {
            this.themeManager = themeManager;

            ApplyTheme();
        }
#endif




        public void ApplyThemeOnClose()
        {
            /*
            if (themeManager.GetCurrentThemeInt() != previousTheme)
            {
                previousTheme = themeManager.GetCurrentThemeInt();
                ApplyTheme();
            }
            else
            {
                SoftApplyTheme();
            }*/
        }

        protected void SoftApplyTheme()
        {

        }
    }
}