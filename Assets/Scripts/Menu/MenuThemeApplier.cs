using System.IO;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Karuta.Objects;
using Karuta.Commons;
using Karuta.UI.CustomButton;
using Karuta.UIComponent;

namespace Karuta.Menu
{
    public class MenuThemeApplier : ThemeApplier
    {
        [Header("Panel Manager")]
        [SerializeField] private PanelManager panelManager;
        protected PanelType currentPanelType;

        [Header("Question")]
        [SerializeField] private Image questionImage;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private List<NumberButton> questionButtons;

        [Header("Option Panels Buttons")]
        [SerializeField] private List<LabeledButton> optionsPanelsButtons;

        [Header("Deck Download")]
        [SerializeField] private DownloadPanelManager downloadPanelManager;
        [SerializeField] private List<MultiLayerButton> downloadButtons;
        [SerializeField] private SelectionButton deleteModeButton;
        [SerializeField] private LabeledToggle selectAllToggle;
        [SerializeField] private Image togglesBackground;

        [Header("Category Buttons")]
        [SerializeField] private List<MultiLayerButton> categoryButtons;

        [Header("Deck Selection")]
        [SerializeField] private DeckSelection deckSelection;
        [SerializeField] private LabeledToggle downloadOnlyToggle;

        

        




        override protected void OnEnable()
        {
            panelManager.PanelUpdateEvent.AddListener(ApplyBackground);
            deckSelection.ContainersCreatedEvent.AddListener(ApplyDeckSelectionContainersColors);
            deckSelection.ButtonsCreatedEvent.AddListener(ApplyDeckSelectionButtonsColors);
            downloadPanelManager.TogglesCreatedEvent.AddListener(ApplyDeckDownloadTogglesColors);

            base.OnEnable();


            themeManager.UpdateThemeEvent.AddListener(SoftApplyTheme);
        }



        protected int previousTheme;







        #region Theme Applier
        protected override void ApplyTheme(Theme currentTheme, BaseTheme baseTheme)
        {
            base.ApplyTheme(currentTheme, baseTheme);

            ApplyCategoryButtonsColors(currentTheme, baseTheme);

            ApplyQuestionColors(currentTheme, baseTheme);

            ApplyOptionPanelsButtonsColors(currentTheme, baseTheme);

            ApplyDeckDownloadColors(currentTheme, baseTheme);

            ApplyDeckSelectionColors(currentTheme, baseTheme);
        }

        #region Background
        private void ApplyBackground(PanelType panelType)
        {
            ApplyBackground(themeManager.GetCurrentTheme(), themeManager.GetBaseTheme(), panelType);
        }

        protected override void ApplyBackground(Theme currentTheme, BaseTheme baseTheme)
        {
            ApplyBackground(currentTheme, baseTheme, currentPanelType);
        }

        private void ApplyBackground(Theme currentTheme, BaseTheme baseTheme, PanelType panelType)
        {
            if (panelType == PanelType.None) { return; }

            currentPanelType = panelType;

            // Switch for a single variable
            Background background = panelType switch
            {
                PanelType.MainMenu => GetBackgroundFromString(currentTheme.mainBackground, baseTheme.mainBackground),
                PanelType.DeckSelection => GetBackgroundFromString(currentTheme.decksSelectionBackground, baseTheme.decksSelectionBackground),
                _ => GetBackgroundFromString(currentTheme.mainBackground, baseTheme.mainBackground),
            };
            
            if (background.isTexture)
            {
                backgroundRawImage.texture = background.texture;
                videoPlayer.Stop();
            }
            else
            {
                long currentFrame = videoPlayer.frame;
                backgroundRawImage.texture = renderTexture;

                Debug.Log(background.videoPath);

                videoPlayer.url = Path.Combine("file://" + LoadManager.ThemesDirectoryPath, background.videoPath);
                videoPlayer.frame = currentFrame;
                videoPlayer.Play();
            }
        }
        #endregion Background

        private void ApplyCategoryButtonsColors(Theme currentTheme, BaseTheme baseTheme)
        {
            foreach (MultiLayerButton button in categoryButtons)
            {
                button.SetColor(0, GetColorFromString(currentTheme.categoryButtonOutlineColor, baseTheme.categoryButtonOutlineColor));
                button.SetColor(1, GetColorFromString(currentTheme.categoryButtonInsideColor, baseTheme.categoryButtonInsideColor));
            }
        }

        private void ApplyQuestionColors(Theme currentTheme, BaseTheme baseTheme)
        {
            questionImage.color = GetColorFromString(currentTheme.questionPanelColor, baseTheme.questionPanelColor);
            questionText.color = GetColorFromString(currentTheme.questionTextColor, baseTheme.questionTextColor);

            foreach (NumberButton button in questionButtons)
            {
                button.SetBaseColor(GetColorFromString(currentTheme.questionNumberPanelColor, baseTheme.questionNumberPanelColor));
                button.SetSelectionColor(GetColorFromString(currentTheme.questionNumberSelectedPanelColor, baseTheme.questionNumberSelectedPanelColor));
                button.GetText().color = GetColorFromString(currentTheme.questionNumberTextColor, baseTheme.questionNumberTextColor);
            }
        }

        private void ApplyOptionPanelsButtonsColors(Theme currentTheme, BaseTheme baseTheme)
        {
            foreach (LabeledButton button in optionsPanelsButtons)
            {
                //button.SetColor(0, GetColorFromString(currentTheme.panelButtonOutlineColor, baseTheme.panelButtonOutlineColor));
                //button.SetColor(1, GetColorFromString(currentTheme.panelButtonInsideColor, baseTheme.panelButtonInsideColor));
                //button.ButtonLabel.color = GetColorFromString(currentTheme.panelButtonTextColor, baseTheme.panelButtonTextColor);
            }
        }

        #region Deck Download
        private void ApplyDeckDownloadColors(Theme currentTheme, BaseTheme baseTheme)
        {
            // Toggles Background
            togglesBackground.color = GetColorFromString(currentTheme.deckDownloadTogglesBackgroundColor, baseTheme.deckDownloadTogglesBackgroundColor);

            // Buttons
            foreach (MultiLayerButton button in downloadButtons)
            {
                button.SetColor(0, GetColorFromString(currentTheme.deckDownloadButtonOutlineColor, baseTheme.deckDownloadButtonOutlineColor));
                button.SetColor(1, GetColorFromString(currentTheme.deckDownloadButtonInsideColor, baseTheme.deckDownloadButtonInsideColor));
            }

            // Select All Toggle
            selectAllToggle.GetText().color = GetColorFromString(currentTheme.selectAllToggleLabelColor, baseTheme.selectAllToggleLabelColor);
            selectAllToggle.GetOutline().effectColor = GetColorFromString(currentTheme.selectAllToggleLabelOutlineColor, baseTheme.selectAllToggleLabelOutlineColor);
            selectAllToggle.GetBackgoundOutline().color = GetColorFromString(currentTheme.selectAllToggleCheckBoxOutlineColor, baseTheme.selectAllToggleCheckBoxOutlineColor);
            selectAllToggle.GetBackgound().color = GetColorFromString(currentTheme.selectAllToggleCheckBoxColor, baseTheme.selectAllToggleCheckBoxColor);
            selectAllToggle.graphic.color = GetColorFromString(currentTheme.selectAllToggleCheckMarkColor, baseTheme.selectAllToggleCheckMarkColor);

            // Delete Mode Button
            deleteModeButton.SetColor(0, GetColorFromString(currentTheme.deleteModeButtonOutlineColor, baseTheme.deleteModeButtonOutlineColor));
            deleteModeButton.SetColor(1, GetColorFromString(currentTheme.deleteModeButtonInsideColor, baseTheme.deleteModeButtonInsideColor));
            deleteModeButton.SetColor(2, GetColorFromString(currentTheme.deleteModeButtonIconColor, baseTheme.deleteModeButtonIconColor));
            deleteModeButton.SetSelectedColor(GetColorFromString(currentTheme.deleteModeButtonSelectedColor, baseTheme.deleteModeButtonSelectedColor));
        }

        private void ApplyDeckDownloadTogglesColors()
        {
            ApplyDeckDownloadTogglesColors(themeManager.GetCurrentTheme(), themeManager.GetBaseTheme());
        }

        private void ApplyDeckDownloadTogglesColors(Theme currentTheme, BaseTheme baseTheme)
        {
            foreach (LabeledToggle toggle in downloadPanelManager.Toggles)
            {
                toggle.GetText().color = GetColorFromString(currentTheme.deckDownloadTogglesLabelColor, baseTheme.deckDownloadTogglesLabelColor);
                toggle.GetOutline().effectColor = GetColorFromString(currentTheme.deckDownloadTogglesLabelOutlineColor, baseTheme.deckDownloadTogglesLabelOutlineColor);
                toggle.GetBackgoundOutline().color = GetColorFromString(currentTheme.deckDownloadTogglesCheckBoxOutlineColor, baseTheme.deckDownloadTogglesCheckBoxOutlineColor);
                toggle.GetBackgound().color = GetColorFromString(currentTheme.deckDownloadTogglesCheckBoxColor, baseTheme.deckDownloadTogglesCheckBoxColor);
                toggle.graphic.color = GetColorFromString(currentTheme.deckDownloadTogglesCheckMarkColor, baseTheme.deckDownloadTogglesCheckMarkColor);
            }
        }
        #endregion Deck Download

        #region Deck Selection
        private void ApplyDeckSelectionColors(Theme currentTheme, BaseTheme baseTheme)
        {
            // Download Only Toggle
            downloadOnlyToggle.GetText().color = GetColorFromString(currentTheme.downloadOnlyToggleLabelColor, baseTheme.downloadOnlyToggleLabelColor);
            downloadOnlyToggle.GetOutline().effectColor = GetColorFromString(currentTheme.downloadOnlyToggleLabelOutlineColor, baseTheme.downloadOnlyToggleLabelOutlineColor);
            downloadOnlyToggle.GetBackgoundOutline().color = GetColorFromString(currentTheme.downloadOnlyToggleCheckBoxOutlineColor, baseTheme.downloadOnlyToggleCheckBoxOutlineColor);
            downloadOnlyToggle.GetBackgound().color = GetColorFromString(currentTheme.downloadOnlyToggleCheckBoxColor, baseTheme.downloadOnlyToggleCheckBoxColor);
            downloadOnlyToggle.graphic.color = GetColorFromString(currentTheme.downloadOnlyToggleCheckMarkColor, baseTheme.downloadOnlyToggleCheckMarkColor);
        }

        private void ApplyDeckSelectionButtonsColors()
        {
            ApplyDeckSelectionButtonsColors(themeManager.GetCurrentTheme(), themeManager.GetBaseTheme());
        }

        private void ApplyDeckSelectionButtonsColors(Theme currentTheme, BaseTheme baseTheme)
        {
            foreach (MultiLayerButton button in deckSelection.Buttons)
            {
                button.SetColor(0, GetColorFromString(currentTheme.deckSelectionButtonInsideColor, baseTheme.deckSelectionButtonInsideColor));
                button.SetColor(1, GetColorFromString(currentTheme.deckSelectionButtonOutlineColor, baseTheme.deckSelectionButtonOutlineColor));
                //button.ButtonLabel.color = GetColorFromString(currentTheme.deckSelectionButtonTextColor, baseTheme.deckSelectionButtonTextColor);
                //button.SetSelectedColor(GetColorFromString(currentTheme.deckSelectionButtonSelectedColor, baseTheme.deckSelectionButtonSelectedColor));
            }
        }

        private void ApplyDeckSelectionContainersColors()
        {
            ApplyDeckSelectionContainersColors(themeManager.GetCurrentTheme(), themeManager.GetBaseTheme());
        }

        private void ApplyDeckSelectionContainersColors(Theme currentTheme, BaseTheme baseTheme)
        {
            foreach(Container container in deckSelection.CategoryContainers)
            {
                container.GetNameTextMesh().color = GetColorFromString(currentTheme.categoryLabelColor, baseTheme.categoryLabelColor);
                container.GetNameTextMesh().outlineColor = GetColorFromString(currentTheme.categoryLabelOutlineColor, baseTheme.categoryLabelOutlineColor);
            }
            foreach (Container container in deckSelection.TypeContainers)
            {
                container.GetNameTextMesh().color = GetColorFromString(currentTheme.typeLabelColor, baseTheme.typeLabelColor);
                container.GetNameTextMesh().outlineColor = GetColorFromString(currentTheme.typeLabelOutlineColor, baseTheme.typeLabelOutlineColor);
            }
        }
        #endregion Deck Selection

        #endregion Theme Applier






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



#if UNITY_EDITOR
        public override void VisualizeApplication(Theme currentTheme, BaseTheme baseTheme)
        {
            base.VisualizeApplication(currentTheme, baseTheme);

            ApplyDeckDownloadTogglesColors(currentTheme, baseTheme);

            ApplyDeckSelectionButtonsColors(currentTheme, baseTheme);

            ApplyDeckSelectionContainersColors(currentTheme, baseTheme);

            foreach (NumberButton button in questionButtons)
            {
                button.image.color = GetColorFromString(currentTheme.questionNumberPanelColor, baseTheme.questionNumberPanelColor);
            }
        }
#endif
    }
}