using System.Net;
using System;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace Karuta.Objects
{
    [System.Serializable]
    public class Background
    {
        public bool isVideo;
        public Texture texture;
        public string videoPath;
    }

    [System.Serializable]
    public class Theme
    {
        [Header("Backgrounds")]
        [SerializeField] private Background mainMenuBackground;
        [SerializeField] private Background decksChoiceBackground;
        [SerializeField] private Background gameBackground;

        [Header("Colors")]
        [SerializeField] private Color mainColor;
        [SerializeField] private Color mainTextColor;
        [SerializeField] private Color secondaryColor;
        [SerializeField] private Color backgroundColorMainMenu;
        [SerializeField] private Color backgroundColorDecksChoice;
        [SerializeField] private Color backgroundColorGame;
        [SerializeField] private Color colorOptionButton;
        [SerializeField] private Color colorOptionButtonText;
        [SerializeField] private Color colorQuitButton;
        [SerializeField] private Color colorPlayPause;
        [SerializeField] private Color colorCheckBox;
        [SerializeField] private Color colorTextNumberPlayersQuestion;
        [SerializeField] private Color colorPanelNumberPlayersQuestion;
        [SerializeField] private Color colorPanelNumberPlayers;
        [SerializeField] private Color colorCentralPanelNumberPlayers;
        [SerializeField] private Color colorTextNumberPlayers;
        [SerializeField] private Color colorOptionPanel;
        [SerializeField] private Color colorOptionPanelBorder;
        [SerializeField] private Color colorOptionsText;
        [SerializeField] private Color colorOptionsTextOutline;
        [SerializeField] private Color colorSliderHandle;
        [SerializeField] private Color colorSliderBackground;
        [SerializeField] private Color colorSliderFill;
        [SerializeField] private Color colorUpdateButton;
        [SerializeField] private Color colorUpdateButtonText;
        [SerializeField] private Color colorContinueArrow;
        [SerializeField] private Color colorBackArrow;
        [SerializeField] private Color colorPanelCurrentPlayer;
        [SerializeField] private Color colorTextCurrentPlayer;
        [SerializeField] private Color colorDeckArrow;
        [SerializeField] private Color colorDeckArrowText;
        [SerializeField] private Color colorNumberOfCardsLeft;
        [SerializeField] private Color colorAnimeTitle;
        [SerializeField] private Color colorCardFoundArrow;
        [SerializeField] private Color colorCardFoundText;
        [SerializeField] private Color colorCardNotFoundArrow;
        [SerializeField] private Color colorCardNotFoundText;
        
        [SerializeField] private Color colorThemesButton;
        [SerializeField] private Color colorThemesQuitButton;
        [SerializeField] private Color colorThemesPanel;
        [SerializeField] private Color colorThemesTitleText;
        [SerializeField] private Color colorThemeSelectedIndicator;
        [SerializeField] private Color colorThemeUnselectedIndicator;

        #region Constructors
        public Theme(JsonTheme theme)
        {
            this.mainMenuBackground.isVideo = theme.mainMenuBackground.Split(".")[^1] == "mp4";
            this.mainMenuBackground.texture = mainMenuBackground.isVideo ? null : LoadManager.LoadThemeVisual(theme.mainMenuBackground);
            this.mainMenuBackground.videoPath = mainMenuBackground.isVideo ? theme.mainMenuBackground : null;

            if (theme.decksChoiceBackground != "")
            {
                this.decksChoiceBackground.isVideo = theme.decksChoiceBackground.Split(".")[^1] == "mp4";
                this.decksChoiceBackground.texture = decksChoiceBackground.isVideo ? null : LoadManager.LoadThemeVisual(theme.decksChoiceBackground);
                this.decksChoiceBackground.videoPath = decksChoiceBackground.isVideo ? theme.decksChoiceBackground : null;
            }
            else
            {
                decksChoiceBackground = null;
            }

            if (theme.gameBackground != "")
            {
                this.gameBackground.isVideo = theme.gameBackground.Split(".")[^1] == "mp4";
                this.gameBackground.texture = gameBackground.isVideo ? null : LoadManager.LoadThemeVisual(theme.gameBackground);
                this.gameBackground.videoPath = gameBackground.isVideo ? theme.gameBackground : null;
            }
            else
            {
                gameBackground = null;
            }

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

        public void Init()
        {
            if (this.mainMenuBackground.texture == null && this.mainMenuBackground.videoPath == "")
            {
                this.mainMenuBackground = null;
            }
            if (this.decksChoiceBackground.texture == null && this.decksChoiceBackground.videoPath == "")
            {
                this.decksChoiceBackground = null;
            }
            if (this.gameBackground.texture == null && this.gameBackground.videoPath == "")
            {
                this.gameBackground = null;
            }
        }
        #endregion Constructors

        #region Getter

        #region Backgrounds
        public Background GetMainMenuBackground()
        {
            return this.mainMenuBackground;
        }

        public Background GetDecksChoiceBackground()
        {
            if (this.decksChoiceBackground == null)
            {
                return this.mainMenuBackground;
            }
            return this.decksChoiceBackground;
        }

        public Background GetGameBackground()
        {
            if (this.gameBackground == null)
            {
                return this.mainMenuBackground;
            }
            return this.gameBackground;
        }
        #endregion Backgrounds

        #endregion Getter
    }
}