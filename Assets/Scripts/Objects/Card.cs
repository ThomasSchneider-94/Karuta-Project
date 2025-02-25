using System.Collections.Generic;
using System;
using UnityEngine;

namespace Karuta.Objects
{
    #region Json Objects
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
    #endregion Json Objects

    public class Card
    {
        public string Deck { get;}
        public string Anime { get;}
        public string CardType { get;}
        public string VisualName { get; }
        public Sprite Visual { get; private set; }
        public bool IsVisualInitialized { get; private set; }
        public string AudioName { get; }

        #region Constructors
        public Card(string deck, string anime, string type, string visualName, Sprite visual, bool visualInitialized, string audioName)
        {
            this.Deck = deck;
            this.Anime = anime;
            this.CardType = type;
            this.VisualName = visualName;
            this.Visual = visual;
            this.IsVisualInitialized = visualInitialized;
            this.AudioName = audioName;
        }

        public Card(string deck, JsonCard jsonCard)
        {
            this.Deck = deck;
            this.Anime = jsonCard.anime;
            this.CardType = jsonCard.type;

            this.VisualName = jsonCard.visual;
            this.Visual = null;
            this.IsVisualInitialized = false;

            this.AudioName = jsonCard.audio;
        }
        #endregion Constructors

        public void InitVisual(Sprite visual, bool isVisualInitialized)
        {
            this.Visual = visual;
            this.IsVisualInitialized = isVisualInitialized;
        }

        public string Dump()
        {
            return string.Format("Deck {0}: {1} - {2}", Deck, Anime, CardType);
        }
    }
}
