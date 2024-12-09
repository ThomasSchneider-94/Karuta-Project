using UnityEngine;

namespace Karuta.Objects
{
    public class Card
    {
        private readonly string deck;
        private readonly string anime;
        private readonly string type;
        private readonly string visualName;
        private Sprite visual;
        private bool visualInitialized;
        private readonly string audioName;

        #region Constructors
        public Card(string deck, string anime, string type, string visualName, Sprite visual, bool visualInitialized, string audioName)
        {
            this.deck = deck;
            this.anime = anime;
            this.type = type;
            this.visualName = visualName;
            this.visual = visual;
            this.visualInitialized = visualInitialized;
            this.audioName = audioName;
        }

        public Card(string deck, JsonCard jsonCard)
        {
            this.deck = deck;
            this.anime = jsonCard.anime;
            this.type = jsonCard.type;

            this.visualName = jsonCard.visual;
            this.visual = null;
            this.visualInitialized = false;

            this.audioName = jsonCard.audio;
        }
        #endregion Constructors

        public void InitVisual(Sprite visual, bool visualInitialized)
        {
            this.visual = visual;
            this.visualInitialized = visualInitialized;
        }

        public string Dump()
        {
            return string.Format("Deck {0}: {1} - {2}", deck, anime, type);
        }

        #region Getter
        public string GetDeck()
        {
            return deck;
        }

        public string GetAnime()
        {
            return anime;
        }

        public string GetCardType()
        {
            return type;
        }

        public string GetVisualName()
        {
            return visualName;
        }

        public Sprite GetVisual()
        {
            return visual;
        }

        public bool IsVisualInitialized()
        {
            return visualInitialized;
        }

        public string GetAudioName()
        {
            return audioName;
        }
        #endregion Getter
    }
}
