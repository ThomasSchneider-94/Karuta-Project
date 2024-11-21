using System;
using UnityEngine;

namespace Karuta.ScriptableObjects
{
    public class DeckInfo : ScriptableObject
    {
        public enum DeckCategory
        {
            KARUTA,
            KARUTO,
            CATEGORY_NB
        }
        public enum DeckType
        {
            CASUAL,
            NORMAL,
            FREESTYLE,
            TYPE_NB
        }

        private string deckName;
        private DeckInfo.DeckCategory category;
        private DeckInfo.DeckType type;
        private Sprite cover;
        private string coverName;
        private bool isDownloaded;

        #region Constructors
        public DeckInfo(string name, DeckInfo.DeckCategory category, DeckType type, Sprite cover, string coverName, bool isDownloaded)
        {
            Init(name, category, type, cover, coverName, isDownloaded);
        }

        public DeckInfo(JsonObjects.JsonDeckInfo jsonDeckInfo)
        {
            Init(jsonDeckInfo);
        }

        public void Init(string name, DeckInfo.DeckCategory category, DeckType type, Sprite cover, string coverName, bool isDownloaded)
        {
            this.deckName = name;
            this.category = category;
            this.type = type;
            this.cover = cover;
            this.coverName = coverName;
            this.isDownloaded = isDownloaded;
        }

        public void Init(JsonObjects.JsonDeckInfo jsonDeckInfo)
        {
            this.deckName = jsonDeckInfo.name;
            this.category = (DeckCategory)jsonDeckInfo.category;
            this.type = (DeckInfo.DeckType)jsonDeckInfo.type;
            this.cover = LoadManager.Instance.LoadCover(jsonDeckInfo.cover);
            this.coverName = jsonDeckInfo.cover;
            this.isDownloaded = jsonDeckInfo.isDownloaded;
        }
        #endregion Constructors
        
        public string Dump()
        {
            return string.Format("Deck {0}: Category {1}; Type {2}; Cover: {3}; isDownloaded: {4}", deckName, category.ToString(), type.ToString(), cover.name, isDownloaded);
        }

        /// <summary>
        /// Return if the deck is greater than the given deck argument
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public bool IsGreaterThan(DeckInfo deck)
        {
            if ((int)category < (int)deck.category)
            {
                return false;
            }
            else if ((int)category > (int)deck.category)
            {
                return true;
            }
            else
            {
                if ((int)type < (int)deck.type)
                {
                    return false;
                }
                else if ((int)type > (int)deck.type)
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
        public DeckCategory GetCategory()
        {
            return category;
        }
        public DeckType GetDeckType()
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