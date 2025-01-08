using UnityEngine;

namespace Karuta.Objects
{
    [System.Serializable]
    public class BaseTheme
    {
        [Header("Backgrounds")]
        public Background mainBackground;
        public Background decksSelectionBackground;
        public Background gameBackground;

        [Header("Question Colors")]
        public Color questionPanelColor;
        public Color questionTextColor;
        public Color questionNumberPanelColor;
        public Color questionNumberSelectedPanelColor;
        public Color questionNumberTextColor;

        [Header("Download Only Toggle Colors")]
        public Color downloadOnlyToggleLabelColor;
        public Color downloadOnlyToggleLabelOutlineColor;
        public Color downloadOnlyToggleCheckBoxOutlineColor;
        public Color downloadOnlyToggleCheckBoxColor;
        public Color downloadOnlyToggleCheckMarkColor;

        [Header("Category Buttons Colors")]
        public Color categoryButtonOutlineColor;
        public Color categoryButtonInsideColor;

        [Header("Arrow Buttons Colors")]
        public Color arrowButtonOutsideColor;
        public Color arrowButtonInsideColor;
        public Color arrowButtonTextColor;
        public Color reverseArrowButtonOutsideColor;
        public Color reverseArrowButtonInsideColor;
        public Color reverseArrowButtonTextColor;

        [Header("Option Buttons Colors")]
        public Color optionButtonOutlineColor;
        public Color optionButtonInsideColor;
        public Color optionButtonIconColor;

        [Header("Option Toggles Colors")]
        public Color optionsTogglesLabelColor;
        public Color optionsTogglesLabelOutlineColor;
        public Color optionsTogglesCheckBoxOutlineColor;
        public Color optionsTogglesCheckBoxColor;
        public Color optionsTogglesCheckMarkColor;

        [Header("Close Buttons Colors")]
        public Color closeButtonOutlineColor;
        public Color closeButtonInsideColor;
        public Color closeButtonIconColor;

        [Header("Panel Buttons Colors")]
        public Color panelButtonOutlineColor;
        public Color panelButtonInsideColor;
        public Color panelButtonTextColor;

        [Header("Option Panel Colors")]
        public Color optionPanelColor;
        public Color optionPanelBorderColor;

        [Header("Deck Selection Button Colors")]
        public Color deckSelectionButtonOutlineColor;
        public Color deckSelectionButtonInsideColor;
        public Color deckSelectionButtonTextColor;
        public Color deckSelectionButtonSelectedColor;

        [Header("Deck Download Buttons Colors")]
        public Color deckDownloadButtonOutlineColor;
        public Color deckDownloadButtonInsideColor;

        [Header("Select All Toggle Colors")]
        public Color selectAllToggleLabelColor;
        public Color selectAllToggleLabelOutlineColor;
        public Color selectAllToggleCheckBoxOutlineColor;
        public Color selectAllToggleCheckBoxColor;
        public Color selectAllToggleCheckMarkColor;

        [Header("Deck Download Toggles Colors")]
        public Color deckDownloadTogglesLabelColor;
        public Color deckDownloadTogglesLabelOutlineColor;
        public Color deckDownloadTogglesCheckBoxOutlineColor;
        public Color deckDownloadTogglesCheckBoxColor;
        public Color deckDownloadTogglesCheckMarkColor;
        public Color deckDownloadTogglesBackgroundColor;

        [Header("Indication Arrows Colors")]
        public Color foundArrowOutlineColor;
        public Color foundArrowInsideColor;
        public Color foundArrowTextColor;
        public Color notFoundArrowOutlineColor;
        public Color notFoundArrowInsideColor;
        public Color notFoundArrowTextColor;

        [Header("Delete Mode Button Colors")]
        public Color deleteModeButtonOutlineColor;
        public Color deleteModeButtonInsideColor;
        public Color deleteModeButtonIconColor;
        public Color deleteModeButtonSelectedColor;

        [Header("Category Label Colors")]
        public Color categoryLabelColor;
        public Color categoryLabelOutlineColor;

        [Header("Type Label Colors")]
        public Color typeLabelColor;
        public Color typeLabelOutlineColor;





        [Header("Colors")]
        [SerializeField] private Color mainColor;
        [SerializeField] private Color mainTextColor;
        [SerializeField] private Color secondaryColor;

        [SerializeField] private Color backgroundColorMainMenu;
        [SerializeField] private Color backgroundColorDecksChoice;
        [SerializeField] private Color backgroundColorGame;

        [SerializeField] private Color colorPlayPause;

        [SerializeField] private Color colorNumberOfCardsLeft;
        [SerializeField] private Color colorAnimeTitle;

        [SerializeField] private Color colorThemesButton;
        [SerializeField] private Color colorThemesQuitButton;
        [SerializeField] private Color colorThemesPanel;
        [SerializeField] private Color colorThemesTitleText;
        [SerializeField] private Color colorThemeSelectedIndicator;
        [SerializeField] private Color colorThemeUnselectedIndicator;

        public void Init()
        {
            if ((this.decksSelectionBackground.isTexture && this.decksSelectionBackground.texture == null)
                || (!this.decksSelectionBackground.isTexture) && string.IsNullOrEmpty(this.decksSelectionBackground.videoPath))
            {
                this.decksSelectionBackground = this.mainBackground;
            }
            if ((this.gameBackground.isTexture && this.gameBackground.texture == null)
                || (!this.gameBackground.isTexture) && string.IsNullOrEmpty(this.gameBackground.videoPath))
            {
                this.gameBackground = this.mainBackground;
            }
        }
    }
}