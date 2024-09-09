using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.Device;

namespace Karuta {

    #region Json Decks
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
    #endregion Json Decks

    public class DeckInfo : ScriptableObject
    {
        public enum DeckCategory
        {
            Karuta,
            Karuto,
            CATEGORY_NB
        }
        public enum DeckType
        {
            Casual,
            Normal
        }

        private readonly string deckName;
        private readonly DeckInfo.DeckCategory category;
        private readonly DeckInfo.DeckType type;
        private readonly Sprite cover;
        private readonly bool isDownloaded;

        #region Construtors
        public DeckInfo(string name, DeckInfo.DeckCategory category, DeckType type, Sprite cover, bool isDownloaded)
        {
            this.deckName = name;
            this.category = category;
            this.type = type;
            this.cover = cover;
            this.isDownloaded = isDownloaded;
        }

        public DeckInfo(JsonDeckInfo jsonDeckInfo)
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