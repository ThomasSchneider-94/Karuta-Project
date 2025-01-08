using UnityEngine;

namespace Karuta.Objects
{
    [System.Serializable]
    public class Theme
    {
        // Backgrounds
        public readonly string mainBackground;
        public string decksSelectionBackground;
        public string gameBackground;

        // Question Color
        public readonly string questionPanelColor;
        public readonly string questionTextColor;
        public readonly string questionNumberPanelColor;
        public readonly string questionNumberSelectedPanelColor;
        public readonly string questionNumberTextColor;

        // Download Only Toggle Colors
        public readonly string downloadOnlyToggleLabelColor;
        public readonly string downloadOnlyToggleLabelOutlineColor;
        public readonly string downloadOnlyToggleCheckBoxOutlineColor;
        public readonly string downloadOnlyToggleCheckBoxColor;
        public readonly string downloadOnlyToggleCheckMarkColor;

        // Category Buttons Colors
        public readonly string categoryButtonOutlineColor;
        public readonly string categoryButtonInsideColor;

        // Arrow Buttons
        public readonly string arrowButtonOutsideColor;
        public readonly string arrowButtonInsideColor;
        public readonly string arrowButtonTextColor;
        public readonly string reverseArrowButtonOutsideColor;
        public readonly string reverseArrowButtonInsideColor;
        public readonly string reverseArrowButtonTextColor;

        // Option Buttons Colors
        public readonly string optionButtonOutlineColor;
        public readonly string optionButtonInsideColor;
        public readonly string optionButtonIconColor;

        // Option Toggles Colors
        public readonly string optionsTogglesLabelColor;
        public readonly string optionsTogglesLabelOutlineColor;
        public readonly string optionsTogglesCheckBoxOutlineColor;
        public readonly string optionsTogglesCheckBoxColor;
        public readonly string optionsTogglesCheckMarkColor;

        // CLose Colors
        public readonly string closeButtonOutlineColor;
        public readonly string closeButtonInsideColor;
        public readonly string closeButtonIconColor;

        // Panel Buttons Colors
        public readonly string panelButtonOutlineColor;
        public readonly string panelButtonInsideColor;
        public readonly string panelButtonTextColor;

        // Option Panel
        public readonly string optionPanelColor;
        public readonly string optionPanelBorderColor;

        // Deck Selection
        public readonly string deckSelectionButtonOutlineColor;
        public readonly string deckSelectionButtonInsideColor;
        public readonly string deckSelectionButtonTextColor;
        public readonly string deckSelectionButtonSelectedColor;

        // Deck Download Buttons
        public readonly string deckDownloadButtonOutlineColor;
        public readonly string deckDownloadButtonInsideColor;

        // Select All Toggle
        public readonly string selectAllToggleLabelColor;
        public readonly string selectAllToggleLabelOutlineColor;
        public readonly string selectAllToggleCheckBoxOutlineColor;
        public readonly string selectAllToggleCheckBoxColor;
        public readonly string selectAllToggleCheckMarkColor;

        // Deck Download Toggles
        public readonly string deckDownloadTogglesLabelColor;
        public readonly string deckDownloadTogglesLabelOutlineColor;
        public readonly string deckDownloadTogglesCheckBoxOutlineColor;
        public readonly string deckDownloadTogglesCheckBoxColor;
        public readonly string deckDownloadTogglesCheckMarkColor;
        public readonly string deckDownloadTogglesBackgroundColor;

        // Indication Arrows Colors
        public readonly string foundArrowOutlineColor;
        public readonly string foundArrowInsideColor;
        public readonly string foundArrowTextColor;
        public readonly string notFoundArrowOutlineColor;
        public readonly string notFoundArrowInsideColor;
        public readonly string notFoundArrowTextColor;

        // Delete Mode Button Colors
        public readonly string deleteModeButtonOutlineColor;
        public readonly string deleteModeButtonInsideColor;
        public readonly string deleteModeButtonIconColor;
        public readonly string deleteModeButtonSelectedColor;

        // Category Label Colors
        public readonly string categoryLabelColor;
        public readonly string categoryLabelOutlineColor;

        // Type Label Colors
        public readonly string typeLabelColor;
        public readonly string typeLabelOutlineColor;










        // Colors
        public readonly Color mainColor;
        public readonly Color mainTextColor;
        public readonly Color secondaryColor;
        public readonly Color backgroundColorMainMenu;
        public readonly Color backgroundColorDecksChoice;
        public readonly Color backgroundColorGame;



        public readonly Color colorPlayPause;

        public readonly Color colorNumberOfCardsLeft;
        public readonly Color colorAnimeTitle;

        

        public readonly Color colorThemesPanel;
        public readonly Color colorThemesTitleText;
        public readonly Color colorThemeSelectedIndicator;
        public readonly Color colorThemeUnselectedIndicator;


        public void Init()
        {
            if (string.IsNullOrEmpty(this.decksSelectionBackground))
            {
                this.decksSelectionBackground = this.mainBackground;
            }
            if (string.IsNullOrEmpty(this.gameBackground))
            {
                this.gameBackground = this.mainBackground;
            }
        }
    }
}