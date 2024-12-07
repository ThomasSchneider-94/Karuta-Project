using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Karuta.Objects
{
    [System.Serializable]
    public class Theme 
    {
        [SerializeField] private Sprite mainMenuBackground;
        [SerializeField] private Sprite decksChoiceBackground;
        [SerializeField] private Sprite gameBackground;

        public Color mainColor;
        public Color mainTextColor;
        public Color secondaryColor;
        public Color backgroundColorMainMenu;
        public Color backgroundColorDecksChoice;
        public Color backgroundColorGame;
        public Color colorOptionButton;
        public Color colorOptionButtonText;
        public Color colorQuitButton;
        public Color colorPlayPause;
        public Color colorCheckBox;
        public Color colorTextNumberPlayersQuestion;
        public Color colorPanelNumberPlayersQuestion;
        public Color colorPanelNumberPlayers;
        public Color colorCentralPanelNumberPlayers;
        public Color colorTextNumberPlayers;
        public Color colorOptionPanel;
        public Color colorOptionPanelBorder;
        public Color colorOptionsText;
        public Color colorOptionsTextOutline;
        public Color colorSliderHandle;
        public Color colorSliderBackground;
        public Color colorSliderFill;
        public Color colorUpdateButton;
        public Color colorUpdateButtonText;
        public Color colorContinueArrow;
        public Color colorBackArrow;
        public Color colorPanelCurrentPlayer;
        public Color colorTextCurrentPlayer;
        public Color colorDeckArrow;
        public Color colorDeckArrowText;
        public Color colorNumberOfCardsLeft;
        public Color colorAnimeTitle;
        public Color colorCardFoundArrow;
        public Color colorCardFoundText;
        public Color colorCardNotFoundArrow;
        public Color colorCardNotFoundText;

        public Color colorThemesButton;
        public Color colorThemesQuitButton;
        public Color colorThemesPanel;
        public Color colorThemesTitleText;
        public Color colorThemeSelectedIndicator;
        public Color colorThemeUnselectedIndicator;

        #region Constructors
        public Theme(JsonTheme theme)
        {
            Init(theme);
        }

        public void Init(JsonTheme theme)
        {
            this.mainMenuBackground = LoadManager.LoadThemeVisual(theme.mainMenuBackground);
            this.decksChoiceBackground = LoadManager.LoadThemeVisual(theme.decksChoiceBackground);
            this.gameBackground = LoadManager.LoadThemeVisual(theme.gameBackground);

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
        public Sprite GetMainMenuBackgroundSprite()
        {
            return mainMenuBackground;
        }

        public Sprite GetDeckSelectionBackgroundSprite()
        {
            if (gameBackground == null)
            {
                return decksChoiceBackground;
            }
            return mainMenuBackground;
        }

        public Sprite GetGameBackgroundSprite()
        {
            if (gameBackground == null)
            {
                return gameBackground;
            }
            return mainMenuBackground;
        }
        #endregion Getter
    }
}