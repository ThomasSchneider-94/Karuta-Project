using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Karuta
{
    #region Json Cards
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
    #endregion Json Cards

    public class Card : ScriptableObject
    {
        private readonly string anime;
        private readonly string type;
        private readonly Sprite visual;
        private readonly string sound; // TODO

        #region Constructors
        public Card(string anime, string type, Sprite visual, string sound)
        {
            this.anime = anime;
            this.type = type;
            this.visual = visual;
            this.sound = sound; // TODO
        }

        public Card(JsonCard jsonCard)
        {
            this.anime = jsonCard.anime;
            this.type = jsonCard.type;

            this.visual = GameManager.Instance.LoadSprite("Visuals", jsonCard.visual);

            this.sound = jsonCard.sound; // TODO
        }
        #endregion Constructors
    }
}
