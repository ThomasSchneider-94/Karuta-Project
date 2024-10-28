using System;
using System.Collections.Generic;
using UnityEngine;

namespace Karuta.ScriptableObjects
{   
    public class JsonObjects : ScriptableObject
    {
        [Serializable]
        public class ConfigData
        {
            public string serverIP;
        }

        /* DOWNLOAD */
        #region Download
        [Serializable]
        public class DownloadDeck
        {
            public string name;
            public string category;
            public string type;
            public string cover;
            public List<DownloadCard> cards;
        }

        [Serializable]
        public class DownloadCard
        {
            public string anime;
            public string type;
            public string visual;
            public string audio;
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
            public bool isVisualDownloaded;
            public string audio;
            public bool isAudioDownloaded;
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
    }
}

