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

        [Header("Option Panel Colors")]
        public Color deckSelectionButtonOutlineColor;
        public Color deckSelectionButtonInsideColor;
        public Color deckSelectionButtonTextColor;

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

        #region Backgrounds Getter
        public Background GetMainBackground()
        {
            return mainBackground;
        }

        public Background GetDecksSelectionBackground()
        {
            if (this.decksSelectionBackground.ignore)
            {
                return mainBackground;
            }
            return this.decksSelectionBackground;
        }

        public Background GetGameBackground()
        {
            if (this.gameBackground.ignore)
            {
                return mainBackground;
            }
            return this.gameBackground;
        }
        #endregion Backgrounds Getter
    }
}