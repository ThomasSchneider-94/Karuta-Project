using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.Device;

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
        private bool isDownloaded;

        #region Construtors
        public DeckInfo(string name, DeckInfo.DeckCategory category, DeckType type, Sprite cover, bool isDownloaded)
        {
            this.deckName = name;
            this.category = category;
            this.type = type;
            this.cover = cover;
            this.isDownloaded = isDownloaded;
        }

        public DeckInfo(JsonObjects.JsonDeckInfo jsonDeckInfo)
        {
            this.deckName = jsonDeckInfo.name;
            this.category = (DeckCategory)jsonDeckInfo.category;
            this.type = (DeckInfo.DeckType)jsonDeckInfo.type;
            this.cover = GameManager.Instance.LoadSprite("Covers", jsonDeckInfo.cover);
            this.isDownloaded = jsonDeckInfo.isDownloaded;
        }

        public void Init(string name, DeckInfo.DeckCategory category, DeckType type, Sprite cover, bool isDownloaded)
        {
            this.deckName = name;
            this.category = category;
            this.type = type;
            this.cover = cover;
            this.isDownloaded = isDownloaded;
        }

        public void Init(JsonObjects.JsonDeckInfo jsonDeckInfo)
        {
            this.deckName = jsonDeckInfo.name;
            this.category = (DeckCategory)jsonDeckInfo.category;
            this.type = (DeckInfo.DeckType)jsonDeckInfo.type;
            this.cover = GameManager.Instance.LoadSprite("Covers", jsonDeckInfo.cover);
            this.isDownloaded = jsonDeckInfo.isDownloaded;
        }
        #endregion Construtors

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
        public bool IsDownloaded()
        {
            return isDownloaded;
        }
        #endregion Getter
    }
}