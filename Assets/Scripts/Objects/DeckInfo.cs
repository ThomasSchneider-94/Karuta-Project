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
        private readonly string deckName;
        private readonly int category;
        private readonly int type;
        private readonly Sprite cover;
        private readonly string coverName;
        private readonly bool isDownloaded;
        
        #region Constructors
        public DeckInfo(string deckName, int category, int type, Sprite cover, string coverName, bool isDownloaded)
        {
            this.deckName = deckName;
            this.category = category;
            this.type = type;
            this.cover = cover;
            this.coverName = coverName;
            this.isDownloaded = isDownloaded;
        }

        public DeckInfo(JsonDeckInfo jsonDeckInfo)
        {
            this.deckName = jsonDeckInfo.name;
            this.category = jsonDeckInfo.category;
            this.type = jsonDeckInfo.type;
            this.cover = LoadManager.Instance.LoadCoverSprite(jsonDeckInfo.cover);
            this.coverName = jsonDeckInfo.cover;
            this.isDownloaded = jsonDeckInfo.isDownloaded;
        }
        #endregion Constructors
        
        public string Dump()
        {
            return string.Format("Deck {0}: Category {1}; Type {2}; Cover: {3}; isDownloaded: {4}", deckName, category, type, coverName, isDownloaded);
        }

        /// <summary>
        /// Return if the deck is greater than the given deck argument
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public bool IsGreaterThan(DeckInfo deck)
        {
            if (category < deck.category)
            {
                return false;
            }
            else if (category > deck.category)
            {
                return true;
            }
            else
            {
                if (type < deck.type)
                {
                    return false;
                }
                else if (type > deck.type)
                {
                    return true;
                }
                else
                {
                    return String.Compare(deckName, deck.deckName) > 0;
                }
            }
        }

        #region Getter
        public string GetName()
        {
            return deckName;
        }

        public int GetCategory()
        {
            return category;
        }

        public int GetDeckType()
        {
            return type;
        }

        public Sprite GetCover()
        {
            return cover;
        }

        public string GetCoverName()
        {
            return coverName;
        }

        public bool IsDownloaded()
        {
            return isDownloaded;
        }
        #endregion Getter
    }
}