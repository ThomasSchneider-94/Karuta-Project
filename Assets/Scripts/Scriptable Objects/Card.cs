using UnityEngine;
using System.Threading.Tasks;
using static Karuta.ScriptableObjects.JsonObjects;
using UnityEngine.Rendering;

namespace Karuta.ScriptableObjects
{
    public class Card : ScriptableObject
    {
        private string deck;
        private string anime;
        private string type;
        private string visualName;
        private Sprite visual;
        private string audioName;

        #region Init
        public void Init(string deck, string anime, string type, string visualName, Sprite visual, string audioName)
        {
            this.deck = deck;
            this.anime = anime;
            this.type = type;

            this.visualName = visualName;
            this.visual = visual;

            this.audioName = audioName;
        }

        public void Init(string deck, JsonObjects.JsonCard jsonCard)
        {
            this.deck = deck;
            this.anime = jsonCard.anime;
            this.type = jsonCard.type;

            this.visualName = jsonCard.visual;
            this.visual = null;

            this.audioName = jsonCard.audio;
        }

        public void InitVisual(Sprite visual)
        {
            this.visual = visual;
        }
        #endregion Init

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

        public string GetAudioName()
        {
            return audioName;
        }
        #endregion Getter
    }
}
