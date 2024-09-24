using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Karuta.ScriptableObjects
{   
    public class JsonObjects : ScriptableObject
    {
        /* DECK */
        #region Deck
        [Serializable]
        public class JsonDeck
        {
            public string name;
            public string category;
            public string type;
            public string cover;
            public List<JsonCard> cards;
        }
        #endregion Deck

        /* CARDS */
        #region Cards

        [Serializable]
        public class JsonCard
        {
            public string anime;
            public string type;
            public string visual;
            public string sound;
        }

        [Serializable]
        public class JsonCards
        {
            public List<JsonCard> cards;
        }
        #endregion Cards

        /* DECK INFORMATION */
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
    }
}

