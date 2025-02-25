using System;
using System.Collections.Generic;
using UnityEngine;

namespace Karuta.Objects
{
    #region Json Objects
    [Serializable]
    public class JsonDeck
    {
        public string name;
        public string category;
        public string type;
        public string cover;
        public List<JsonCard> cards;
    }

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
    #endregion Json Objects

    public class DeckInfo
    {
        public string DeckName { get; }
        public int Category { get; }
        public int DeckType { get; }
        public Sprite Cover { get; }
        public string CoverName { get; }
        public bool IsDownloaded { get; }

        #region Constructor
        public DeckInfo(string deckName, int category, int deckType, Sprite cover, string coverName, bool isDownloaded)
        {
            this.DeckName = deckName;
            this.Category = category;
            this.DeckType = deckType;
            this.Cover = cover;
            this.CoverName = coverName;
            this.IsDownloaded = isDownloaded;
        }

        public DeckInfo(JsonDeckInfo jsonDeckInfo)
        {
            this.DeckName = jsonDeckInfo.name;
            this.Category = jsonDeckInfo.category;
            this.DeckType = jsonDeckInfo.type;
            this.Cover = LoadManager.Instance.LoadCoverFromFile(jsonDeckInfo.cover);
            this.CoverName = jsonDeckInfo.cover;
            this.IsDownloaded = jsonDeckInfo.isDownloaded;
        }
        #endregion Constructors

        public string Dump()
        {
            return string.Format("Deck {0}: Category {1}; Type {2}; Cover: {3}; isDownloaded: {4}", DeckName, DecksManager.Instance.GetCategoryName(Category), DecksManager.Instance.GetTypeName(DeckType), CoverName, IsDownloaded);
        }

        /// <summary>
        /// Return if the Deck is greater than the given Deck argument
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public bool IsGreaterThan(DeckInfo deck)
        {
            if (Category < deck.Category)
            {
                return false;
            }
            else if (Category > deck.Category)
            {
                return true;
            }
            else
            {
                if (DeckType < deck.DeckType)
                {
                    return false;
                }
                else if (DeckType > deck.DeckType)
                {
                    return true;
                }
                else
                {
                    return String.Compare(DeckName, deck.DeckName) > 0;
                }
            }
        }
    }
}