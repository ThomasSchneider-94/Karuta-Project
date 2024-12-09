using System;
using System.Collections.Generic;

namespace Karuta.Objects
{
    [Serializable]
    public class ConfigData
    {
        public string serverIP;
    }

    /* CATEGORIES AND TYPES */
    #region Categories and Types
    [Serializable]
    public class CategoriesAndTypes
    {
        public List<Category> categories;
        public List<string> types;
    }

    [Serializable]
    public class Category
    {
        public string name;
        public string icon;
    }
    #endregion Categories and Types

    /* DOWNLOAD */
    #region Download
    [Serializable]
    public class DownloadDeck
    {
        public string name;
        public string category;
        public string type;
        public string cover;
        public List<JsonCard> cards;
    }
    #endregion Download

    /* CARDS */
    #region Card Information

    [Serializable]
    public class JsonCard
    {
        public string anime;
        public string type;
        public string visual;
        public string audio;
    }

    [Serializable]
    public class JsonCards
    {
        public List<JsonCard> cards;
    }
    #endregion Cards

    /* DECK */
    #region Deck Information

    [Serializable]
    public class JsonDeckInfo
    {
        public string name;
        public int category;
        public int type;
        public string cover;
        public bool isDownloaded;
    }

    [Serializable]
    public class JsonDeckInfoList
    {
        public List<JsonDeckInfo> deckInfoList;
    }
    #endregion Deck Information

    /* THEMES */
    #region Themes
    [Serializable]
    public class LightJsonTheme
    {
        public string mainMenuBackground;
        public string decksChoiceBackground;
    }

    [Serializable]
    public class JsonTheme
    {
        public string mainMenuBackground;
        public string decksChoiceBackground;
        public string gameBackground;

        public string mainColor;
        public string mainTextColor;
        public string secondaryColor;
        public string backgroundColorMainMenu;
        public string backgroundColorDecksChoice;
        public string backgroundColorGame;
        public string colorOptionButton;
        public string colorOptionButtonText;
        public string colorQuitButton;
        public string colorPlayPause;
        public string colorCheckBox;
        public string colorTextNumberPlayersQuestion;
        public string colorPanelNumberPlayersQuestion;
        public string colorPanelNumberPlayers;
        public string colorCentralPanelNumberPlayers;
        public string colorTextNumberPlayers;
        public string colorOptionPanel;
        public string colorOptionPanelBorder;
        public string colorOptionsText;
        public string colorOptionsTextOutline;
        public string colorSliderHandle;
        public string colorSliderBackground;
        public string colorSliderFill;
        public string colorUpdateButton;
        public string colorUpdateButtonText;
        public string colorContinueArrow;
        public string colorBackArrow;
        public string colorPanelCurrentPlayer;
        public string colorTextCurrentPlayer;
        public string colorDeckArrow;
        public string colorDeckArrowText;
        public string colorNumberOfCardsLeft;
        public string colorAnimeTitle;
        public string colorCardFoundArrow;
        public string colorCardFoundText;
        public string colorCardNotFoundArrow;
        public string colorCardNotFoundText;

        public string colorThemesButton;
        public string colorThemesQuitButton;
        public string colorThemesPanel;
        public string colorThemesTitleText;
        public string colorThemeSelectedIndicator;
        public string colorThemeUnselectedIndicator;
    }
    #endregion Themes
}

