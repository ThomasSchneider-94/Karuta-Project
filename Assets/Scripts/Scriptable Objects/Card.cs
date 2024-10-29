using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static Karuta.ScriptableObjects.DeckInfo;

namespace Karuta.ScriptableObjects
{
    public class Card : ScriptableObject
    {
        private string anime;
        private string type;
        private Sprite visual;
        private bool isVisualDowloaded;
        private string audio; // TODO
        private bool isAudioDownloaded;

        #region Constructors
        public Card(string anime, string type, Sprite visual, bool isVisualDowloaded, string audio, bool isAudioDownloaded)
        {
            this.anime = anime;
            this.type = type;

            this.visual = visual;
            this.isVisualDowloaded = isVisualDowloaded;

            this.audio = audio; // TODO
            this.isAudioDownloaded = isAudioDownloaded;
        }

        public Card(JsonObjects.JsonCard jsonCard)
        {
            this.anime = jsonCard.anime;
            this.type = jsonCard.type;

            this.visual = GameManager.Instance.LoadSprite(GameManager.visualsDirectoryPath, jsonCard.visual);
            this.isVisualDowloaded = jsonCard.isVisualDownloaded;

            this.audio = jsonCard.audio; // TODO
            this.isAudioDownloaded = jsonCard.isAudioDownloaded;
        }

        public void Init(string anime, string type, Sprite visual, bool isVisualDowloaded, string audio, bool isAudioDownloaded)
        {
            this.anime = anime;
            this.type = type;

            this.visual = visual;
            this.isVisualDowloaded = isVisualDowloaded;

            this.audio = audio; // TODO
            this.isAudioDownloaded = isAudioDownloaded;
        }

        public void Init(JsonObjects.JsonCard jsonCard)
        {
            this.anime = jsonCard.anime;
            this.type = jsonCard.type;

            this.visual = GameManager.Instance.LoadSprite(GameManager.visualsDirectoryPath, jsonCard.visual);
            this.isVisualDowloaded = jsonCard.isVisualDownloaded;

            this.audio = jsonCard.audio; // TODO
            this.isAudioDownloaded = jsonCard.isAudioDownloaded;
        }
        #endregion Constructors

        #region Getter
        public string GetAnime()
        {
            return anime;
        }
        public string GetCardType()
        {
            return type;
        }
        public Sprite GetVisual()
        {
            return visual;
        }
        public bool IsVisualDownloaded()
        {
            return isVisualDowloaded;
        }
        public string GetAudio()
        {
            return audio;
        }
        public bool IsAudioDownloaded()
        {
            return isAudioDownloaded;
        }
        #endregion Getter
    }
}
