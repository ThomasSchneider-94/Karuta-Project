using Karuta.ScriptableObjects;
using Karuta.UIComponent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static Karuta.ScriptableObjects.JsonObjects;
using TMPro;
using System.Threading.Tasks;
using System.Collections;
using System;

namespace Karuta.Game
{
    public class MainGame : MonoBehaviour
    {
        private LoadManager loadManager;
        private OptionsManager optionsManager; 

        [SerializeField] private TextMeshProUGUI cardCounter;
        [SerializeField] private AudioSource audioSource;

        [Header("Card Information Display")]
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI informationTextMesh;
        private Vector2 maxImageSize;

        [Header("Play / Pause")]
        [SerializeField] private MultiLayerButton pauseButton;
        [SerializeField] private Sprite playSprite;
        [SerializeField] private Sprite pauseSprite;
        private bool pause;

        [Header("Hide Answers")]
        [SerializeField] private Button cardButton;
        [SerializeField] private Sprite hidingSprite;
        private bool cardHiden;

        [Header("Download")]
        [SerializeField] private int downloadTimeout;

        private readonly List<Card> cards = new();
        private Card currentCard;

        private void OnEnable()
        {
            loadManager = LoadManager.Instance;
            optionsManager = OptionsManager.Instance;

            optionsManager.UpdateHidenAnswerEvent.AddListener(SetCardVisualAndInformation);

            maxImageSize = cardImage.rectTransform.sizeDelta;

            // Initialize
            if (DecksManager.Instance.IsInitialized())
            {
                ResetGame();
            }
            else
            {
                DecksManager.Instance.DeckListInitializedEvent.AddListener(ResetGame);
            }
        }

        #region Initialize
        public void ResetGame()
        {
            cards.Clear();
            Debug.Log("Init game");

            Initialize();
        }

        private void Initialize()
        {
            foreach (int deckIndex in DecksManager.Instance.GetSelectedDecks())
            {
                DeckInfo deck = DecksManager.Instance.GetDeck(deckIndex);

                JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, deck.GetCategory().ToString(), deck.GetName() + ".json")));

                foreach (JsonCard jsonCard in jsonCards.cards)
                {
                    Card card = ScriptableObject.CreateInstance<Card>();
                    card.Init(deck.GetName(), jsonCard);
                    cards.Add(card);
                }
            }

            cardCounter.text = "Cartes restantes: " + cards.Count.ToString();
            Debug.Log(cards.Count);
            PlayCard();
        }
        #endregion Initialize

        private void PlayCard()
        {
            audioSource.Stop();

            // Select Card
            currentCard = cards[UnityEngine.Random.Range(0, cards.Count)];
            cardHiden = optionsManager.AreAnswersHiden();

            loadManager.LoadAudio(currentCard.GetAudioName(), OnAudioLoaded);
            if (currentCard.GetVisual() == null)
            {
                loadManager.LoadVisual(currentCard.GetVisualName(), OnVisualLoaded);
            }
            else
            {
                SetCardVisualAndInformation();
            }
        }

        private void OnVisualLoaded(Sprite sprite)
        {
            if (sprite != null)
            {
                currentCard.InitVisual(sprite);
                SetCardVisualAndInformation();
            }
            else
            {
                Debug.LogError("Failed to load visual");
            }
        }

        private void OnAudioLoaded(AudioClip audioClip)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Failed to load audio clip");
            }
        }

        #region Card Visual and Information
        private void SetCardVisualAndInformation()
        {
            cardButton.interactable = optionsManager.AreAnswersHiden();
            if (optionsManager.AreAnswersHiden() && cardHiden)
            {
                HideCard();
            }
            else
            {
                RevealCard();
            }
        }

        private void HideCard()
        {
            // Visual
            float scale = Mathf.Min(maxImageSize.x / hidingSprite.textureRect.width, maxImageSize.y / hidingSprite.textureRect.height);
            cardImage.rectTransform.sizeDelta = hidingSprite.textureRect.size;
            cardImage.rectTransform.localScale = new Vector2(scale, scale);
            cardImage.sprite = hidingSprite;

            // Information text
            informationTextMesh.text = "???\n???\n???";
        }

        private void RevealCard()
        {
            // Visual
            Sprite sprite = currentCard.GetVisual();
            float scale = Mathf.Min(maxImageSize.x / sprite.textureRect.width, maxImageSize.y / sprite.textureRect.height);
            cardImage.rectTransform.sizeDelta = sprite.textureRect.size;
            cardImage.rectTransform.localScale = new Vector2(scale, scale);
            cardImage.sprite = sprite;

            // Information text
            informationTextMesh.text = currentCard.GetAnime() + "\n" + currentCard.GetCardType() + "\nDeck " + currentCard.GetDeck();
        }

        public void ChangeCardState()
        {
            if (!optionsManager.AreAnswersHiden()) { return; }
            cardHiden = !cardHiden;
            SetCardVisualAndInformation();
        }
        #endregion Card Visual and Information







        public void NextCard(bool found) 
        {
            if (found)
            {
                cards.Remove(currentCard);
            }
            cardCounter.text = "Cartes restantes: " + cards.Count.ToString();
            PlayCard();
        }

        public void TogglePause()
        {
            pause = !pause;
            if (pause)
            {
                pauseButton.SetIconSprite(pauseSprite);
            }
            else
            {
                pauseButton.SetIconSprite(playSprite);
            }
        }
    }
}