using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Karuta.ScriptableObjects
{
    #region Json Cards
    [Serializable]

    #endregion Json Cards

    public class Card : ScriptableObject
    {
        private string anime;
        private string type;
        private Sprite visual;
        private string sound; // TODO

        #region Constructors
        public Card(string anime, string type, Sprite visual, string sound)
        {
            this.anime = anime;
            this.type = type;
            this.visual = visual;
            this.sound = sound; // TODO
        }

        public Card(JsonObjects.JsonCard jsonCard)
        {
            this.anime = jsonCard.anime;
            this.type = jsonCard.type;

            this.visual = GameManager.Instance.LoadSprite("Visuals", jsonCard.visual);

            this.sound = jsonCard.sound; // TODO
        }

        public void Init(string anime, string type, Sprite visual, string sound)
        {
            this.anime = anime;
            this.type = type;
            this.visual = visual;
            this.sound = sound; // TODO
        }

        public void Init(JsonObjects.JsonCard jsonCard)
        {
            this.anime = jsonCard.anime;
            this.type = jsonCard.type;

            this.visual = GameManager.Instance.LoadSprite("Visuals", jsonCard.visual);

            this.sound = jsonCard.sound; // TODO
        }
        #endregion Constructors
    }
}
