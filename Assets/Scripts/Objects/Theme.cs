using System.Net;
using System;
using UnityEngine;

namespace Karuta.Objects
{
    [System.Serializable]
    public class Theme 
    {
        // Backgrounds
        private readonly string mainMenuBackgroundPath;
        private readonly string decksChoiceBackgroundPath;
        private readonly string gameBackgroundPath;

        #region Colors
        private Color mainColor;
        private Color mainTextColor;
        private Color secondaryColor;
        private Color backgroundColorMainMenu;
        private Color backgroundColorDecksChoice;
        private Color backgroundColorGame;
        private Color colorOptionButton;
        private Color colorOptionButtonText;
        private Color colorQuitButton;
        private Color colorPlayPause;
        private Color colorCheckBox;
        private Color colorTextNumberPlayersQuestion;
        private Color colorPanelNumberPlayersQuestion;
        private Color colorPanelNumberPlayers;
        private Color colorCentralPanelNumberPlayers;
        private Color colorTextNumberPlayers;
        private Color colorOptionPanel;
        private Color colorOptionPanelBorder;
        private Color colorOptionsText;
        private Color colorOptionsTextOutline;
        private Color colorSliderHandle;
        private Color colorSliderBackground;
        private Color colorSliderFill;
        private Color colorUpdateButton;
        private Color colorUpdateButtonText;
        private Color colorContinueArrow;
        private Color colorBackArrow;
        private Color colorPanelCurrentPlayer;
        private Color colorTextCurrentPlayer;
        private Color colorDeckArrow;
        private Color colorDeckArrowText;
        private Color colorNumberOfCardsLeft;
        private Color colorAnimeTitle;
        private Color colorCardFoundArrow;
        private Color colorCardFoundText;
        private Color colorCardNotFoundArrow;
        private Color colorCardNotFoundText;

        private Color colorThemesButton;
        private Color colorThemesQuitButton;
        private Color colorThemesPanel;
        private Color colorThemesTitleText;
        private Color colorThemeSelectedIndicator;
        private Color colorThemeUnselectedIndicator;
        #endregion Colors

        #region Constructors
        public Theme(JsonTheme theme)
        {
            this.mainMenuBackgroundPath = theme.mainMenuBackground;
            this.decksChoiceBackgroundPath = theme.decksChoiceBackground;
            this.gameBackgroundPath = theme.gameBackground;

            ColorUtility.TryParseHtmlString(theme.mainColor, out this.mainColor);
            ColorUtility.TryParseHtmlString(theme.mainTextColor, out this.mainTextColor);
            ColorUtility.TryParseHtmlString(theme.secondaryColor, out this.secondaryColor);
            ColorUtility.TryParseHtmlString(theme.backgroundColorMainMenu, out this.backgroundColorMainMenu);
            ColorUtility.TryParseHtmlString(theme.backgroundColorDecksChoice, out this.backgroundColorDecksChoice);
            ColorUtility.TryParseHtmlString(theme.backgroundColorGame, out this.backgroundColorGame);
            ColorUtility.TryParseHtmlString(theme.colorOptionButton, out this.colorOptionButton);
            ColorUtility.TryParseHtmlString(theme.colorOptionButtonText, out this.colorOptionButtonText);
            ColorUtility.TryParseHtmlString(theme.colorQuitButton, out this.colorQuitButton);
            ColorUtility.TryParseHtmlString(theme.colorPlayPause, out this.colorPlayPause);
            ColorUtility.TryParseHtmlString(theme.colorCheckBox, out this.colorCheckBox);
            ColorUtility.TryParseHtmlString(theme.colorTextNumberPlayersQuestion, out this.colorTextNumberPlayersQuestion);
            ColorUtility.TryParseHtmlString(theme.colorPanelNumberPlayersQuestion, out this.colorPanelNumberPlayersQuestion);
            ColorUtility.TryParseHtmlString(theme.colorPanelNumberPlayers, out this.colorPanelNumberPlayers);
            ColorUtility.TryParseHtmlString(theme.colorCentralPanelNumberPlayers, out this.colorCentralPanelNumberPlayers);
            ColorUtility.TryParseHtmlString(theme.colorTextNumberPlayers, out this.colorTextNumberPlayers);
            ColorUtility.TryParseHtmlString(theme.colorOptionPanel, out this.colorOptionPanel);
            ColorUtility.TryParseHtmlString(theme.colorOptionPanelBorder, out this.colorOptionPanelBorder);
            ColorUtility.TryParseHtmlString(theme.colorOptionsText, out this.colorOptionsText);
            ColorUtility.TryParseHtmlString(theme.colorOptionsTextOutline, out this.colorOptionsTextOutline);
            ColorUtility.TryParseHtmlString(theme.colorSliderHandle, out this.colorSliderHandle);
            ColorUtility.TryParseHtmlString(theme.colorSliderBackground, out this.colorSliderBackground);
            ColorUtility.TryParseHtmlString(theme.colorSliderFill, out this.colorSliderFill);
            ColorUtility.TryParseHtmlString(theme.colorUpdateButton, out this.colorUpdateButton);
            ColorUtility.TryParseHtmlString(theme.colorUpdateButtonText, out this.colorUpdateButtonText);
            ColorUtility.TryParseHtmlString(theme.colorContinueArrow, out this.colorContinueArrow);
            ColorUtility.TryParseHtmlString(theme.colorBackArrow, out this.colorBackArrow);
            ColorUtility.TryParseHtmlString(theme.colorPanelCurrentPlayer, out this.colorPanelCurrentPlayer);
            ColorUtility.TryParseHtmlString(theme.colorTextCurrentPlayer, out this.colorTextCurrentPlayer);
            ColorUtility.TryParseHtmlString(theme.colorDeckArrow, out this.colorDeckArrow);
            ColorUtility.TryParseHtmlString(theme.colorDeckArrowText, out this.colorDeckArrowText);
            ColorUtility.TryParseHtmlString(theme.colorNumberOfCardsLeft, out this.colorNumberOfCardsLeft);
            ColorUtility.TryParseHtmlString(theme.colorAnimeTitle, out this.colorAnimeTitle);
            ColorUtility.TryParseHtmlString(theme.colorCardFoundArrow, out this.colorCardFoundArrow);
            ColorUtility.TryParseHtmlString(theme.colorCardFoundText, out this.colorCardFoundText);
            ColorUtility.TryParseHtmlString(theme.colorCardNotFoundArrow, out this.colorCardNotFoundArrow);
            ColorUtility.TryParseHtmlString(theme.colorCardNotFoundText, out this.colorCardNotFoundText);

            ColorUtility.TryParseHtmlString(theme.colorThemesButton, out this.colorThemesButton);
            ColorUtility.TryParseHtmlString(theme.colorThemesQuitButton, out this.colorThemesQuitButton);
            ColorUtility.TryParseHtmlString(theme.colorThemesPanel, out this.colorThemesPanel);
            ColorUtility.TryParseHtmlString(theme.colorThemesTitleText, out this.colorThemesTitleText);
            ColorUtility.TryParseHtmlString(theme.colorThemeSelectedIndicator, out this.colorThemeSelectedIndicator);
            ColorUtility.TryParseHtmlString(theme.colorThemeUnselectedIndicator, out this.colorThemeUnselectedIndicator);
        }
        #endregion Constructors

        #region Getter

        #region Backgrounds
        public string GetMainMenuBackgroundPath()
        {
            return this.mainMenuBackgroundPath;
        }

        public string GetDecksChoiceBackgroundPath()
        {
            if (this.decksChoiceBackgroundPath == null)
            {
                return this.mainMenuBackgroundPath;
            }
            return this.decksChoiceBackgroundPath;
        }

        public string GetGameBackgroundPath()
        {
            if (this.gameBackgroundPath == null)
            {
                return this.mainMenuBackgroundPath;
            }
            return this.gameBackgroundPath;
        }
        #endregion Backgrounds

        #endregion Getter
    }
}