using Karuta.Commons;
using Karuta.UIComponent;
using System.IO;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Karuta.Menu
{
    public class MenuThemeApplier : ThemeApplier
    {
        [Header("Background")]
        [SerializeField] private PanelManager panelManager;

        [Header("Question")]
        [SerializeField] private Image questionImage;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private List<NumberButton> questionButtons;

        [Header("Buttons")]
        [SerializeField] private List<MultiLayerButton> categoryButtons;
        [SerializeField] private List<MultiLayerButton> panelButtons;

        [Header("Toggles")]
        [SerializeField] private LabeledToggle downloadOnlyToggle;

        [Header("Deck Selection")]
        [SerializeField] private DeckSelection deckSelection;

        [Header("Deck Download")]
        [SerializeField] private DeckDownload deckDownload;
        [SerializeField] private List<MultiLayerButton> downloadButtons;
        [SerializeField] private LabeledToggle selectAllToggle;
        [SerializeField] private Image togglesBackground;



        protected PanelType currentPanelType;

        override protected void OnEnable()
        {
            panelManager.PanelUpdateEvent.AddListener(ApplyBackground);

            base.OnEnable();
        }





        public override void ApplyTheme()
        {
            base.ApplyTheme();

            ApplyMenuButtons();

            ApplyMainPanelColors();

            ApplyDeckSelectionColors();

            ApplyDeckDownloadColors();
        }

        protected override void ApplyBackground()
        {
            ApplyBackground(currentPanelType);
        }

        private void ApplyBackground(PanelType panelType)
        {
            if (panelType == PanelType.None) { return; }

            currentPanelType = panelType;

            // Switch for a single variable
            Background background = panelType switch
            {
                PanelType.MainMenu => themeManager.GetMainBackground(),
                PanelType.DeckSelection => themeManager.GetDecksSelectionBackground(),
                _ => themeManager.GetMainBackground(),
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

        private void ApplyMenuButtons()
        {
            // Category Buttons
            foreach (MultiLayerButton button in categoryButtons)
            {
                button.SetColor(0, themeManager.GetCategoryButtonOutlineColor());
                button.SetColor(1, themeManager.GetCategoryButtonInsdeColor());
            }

            // Panel Buttons
            foreach (MultiLayerButton button in panelButtons)
            {
                button.SetColor(0, themeManager.GetPanelButtonOutlineColor());
                button.SetColor(1, themeManager.GetPanelButtonInsideColor());
                button.GetLabel().color = themeManager.GetPanelButtonTextColor();
            }
        }

        private void ApplyMainPanelColors()
        {
            questionImage.color = themeManager.GetQuestionPanelColor();
            questionText.color = themeManager.GetQuestionTextColor();

            foreach (NumberButton button in questionButtons)
            {
                button.SetBaseColor(themeManager.GetQuestionNumberPanelColor());
                button.SetSelectionColor(themeManager.GetQuestionNumberSelectedPanelColor());
                button.GetText().color = themeManager.GetQuestionNumberTextColor();
            }
        }

        private void ApplyDeckSelectionColors()
        {
            foreach (SelectionButton button in deckSelection.GetSelectionButtons())
            {
                button.SetColor(0, themeManager.GetDeckSelectionButtonOutlineColor());
                button.SetColor(1, themeManager.GetDeckSelectionButtonInsideColor());
                button.GetLabel().color = themeManager.GetDeckSelectionButtonTextColor();
            }

            // Download Only Toggle
            downloadOnlyToggle.GetText().color = themeManager.GetDownloadOnlyToggleLabelColor();
            downloadOnlyToggle.GetOutline().effectColor = themeManager.GetDownloadOnlyToggleLabelOutlineColor();
            downloadOnlyToggle.GetBackgoundOutline().color = themeManager.GetDownloadOnlyToggleCheckBoxOutlineColor();
            downloadOnlyToggle.GetBackgound().color = themeManager.GetDownloadOnlyToggleCheckBoxColor();
            downloadOnlyToggle.graphic.color = themeManager.GetDownloadOnlyToggleCheckMarkColor();
        }

        private void ApplyDeckDownloadColors()
        {
            // Download Toggles
            foreach (LabeledToggle toggle in deckDownload.GetToggles())
            {
                toggle.GetText().color = themeManager.GetDeckDownloadTogglesLabelColor();
                toggle.GetOutline().effectColor = themeManager.GetDeckDownloadTogglesLabelOutlineColor();
                toggle.GetBackgoundOutline().color = themeManager.GetDeckDownloadTogglesCheckBoxOutlineColor();
                toggle.GetBackgound().color = themeManager.GetDeckDownloadTogglesCheckBoxColor();
                toggle.graphic.color = themeManager.GetDeckDownloadTogglesCheckMarkColor();
            }

            // Toggles Background
            togglesBackground.color = themeManager.GetDeckDownloadTogglesBackgroundColor();

            // Buttons
            foreach (MultiLayerButton button in downloadButtons)
            {
                button.SetColor(0, themeManager.GetDeckDownloadButtonOutlineColor());
                button.SetColor(1, themeManager.GetDeckDownloadButtonInsideColor());
            }

            // Select All Toggle
            selectAllToggle.GetText().color = themeManager.GetSelectAllToggleLabelColor();
            selectAllToggle.GetOutline().effectColor = themeManager.GetSelectAllToggleLabelOutlineColor();
            selectAllToggle.GetBackgoundOutline().color = themeManager.GetSelectAllToggleCheckBoxOutlineColor();
            selectAllToggle.GetBackgound().color = themeManager.GetSelectAllToggleCheckBoxColor();
            selectAllToggle.graphic.color = themeManager.GetSelectAllToggleCheckMarkColor();
        }










#if UNITY_EDITOR
        public override void VisualizeApplication(ThemeManager themeManager)
        {
            base.VisualizeApplication(themeManager);

            foreach (NumberButton button in questionButtons)
            {
                button.image.color = themeManager.GetQuestionNumberPanelColor();
            }
        }
#endif
    }
}