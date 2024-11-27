using Karuta.ScriptableObjects;
using Karuta.UIComponent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static Karuta.ScriptableObjects.JsonObjects;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Karuta.Game
{
    public class MainGame : MonoBehaviour
    {
        private LoadManager loadManager;
        private OptionsManager optionsManager; 

        [SerializeField] private TextMeshProUGUI cardCounter;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private GameObject gamePanel;

        [Header("Card Information Display")]
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI informationTextMesh;
        private Vector2 maxImageSize;

        [Header("Play / Pause")]
        [SerializeField] private MultiLayerButton pauseButton;
        [SerializeField] private Sprite playSprite;
        [SerializeField] private Sprite pauseSprite;
        private bool pause = false;

        [Header("Hide Answers")]
        [SerializeField] private Button cardButton;
        [SerializeField] private Sprite hidingSprite;
        private bool cardHiden;

        [Header("Card Appears")]
        [SerializeField] private float verticalOffset;
        [SerializeField] private float appearTime;

        [Header("Find")]
        [SerializeField] private float maxRotation;
        [Min(1)]
        [SerializeField] private float rotationDivisor;
        [SerializeField] private float returnToPositionTime;
        [SerializeField] private float angleThreshold;

        [Header("Indications")]
        [SerializeField] private float indicationsFadeDivisor;
        [SerializeField] private Image foundImage;
        [SerializeField] private TextMeshProUGUI foundText;
        [SerializeField] private Image notFoundImage;
        [SerializeField] private TextMeshProUGUI notFoundText;
        [SerializeField] private Color foundColor;
        [SerializeField] private Color notFoundColor;

        private readonly List<Card> cards = new();
        private Card currentCard;
        private Vector2 cardBasePosition;
        private bool cardMoving = false;

        private void OnEnable()
        {
            loadManager = LoadManager.Instance;
            optionsManager = OptionsManager.Instance;

            optionsManager.UpdateHidenAnswerEvent.AddListener(UpdateHidenProperty);

            maxImageSize = cardImage.rectTransform.sizeDelta;
            cardBasePosition = cardImage.rectTransform.localPosition;

            // Set indication color to transparent
            Color tempFoundColor = foundColor;
            tempFoundColor.a = 0;
            foundColor = tempFoundColor;

            Color tempNotFoundColor = notFoundColor;
            tempNotFoundColor.a = 0;
            notFoundColor = tempNotFoundColor;

            ApplyIndicationColors();

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
        /// <summary>
        /// Reset the game
        /// </summary>
        private void ResetGame()
        {
            cards.Clear();

            Initialize();
        }

        /// <summary>
        /// Initialize all the cards from the decks in decksManager
        /// </summary>
        private void Initialize()
        {
            cardButton.interactable = optionsManager.AreAnswersHiden();

            foreach (int deckIndex in DecksManager.Instance.GetSelectedDecks())
            {
                DeckInfo deck = DecksManager.Instance.GetDeck(deckIndex);

                JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategory(deck.GetCategory()), deck.GetName() + ".json")));

                foreach (JsonCard jsonCard in jsonCards.cards)
                {
                    Card card = ScriptableObject.CreateInstance<Card>();
                    card.Init(deck.GetName(), jsonCard);
                    cards.Add(card);
                }
            }

            DecksManager.Instance.ClearSelected();

            cardCounter.text = "Cartes restantes: " + cards.Count.ToString();
            //Debug.Log(cards.Count)
            PlayCard();
        }
        #endregion Initialize

        #region Play Card
        /// <summary>
        ///  Play a random card
        /// </summary>
        private void PlayCard()
        {
            // Reset the card parameters
            StopAllCoroutines();
            audioSource.Stop();

            Color tmpColor = cardImage.color;
            tmpColor.a = 0;
            cardImage.color = tmpColor;

            // Select Card
            currentCard = cards[UnityEngine.Random.Range(0, cards.Count)];

            cardButton.interactable = false;
            cardHiden = optionsManager.AreAnswersHiden();

            // Load visual and audio
            loadManager.LoadAudio(currentCard.GetAudioName(), OnAudioLoaded);
            if (currentCard.GetVisual() == null)
            {
                loadManager.LoadVisual(currentCard.GetVisualName(), OnVisualLoaded);
            }
            else
            {
                SetCardVisualAndInformation();
                StartCoroutine(CardAppear());
            }
        }

        /// <summary>
        /// Called when visual loaded
        /// </summary>
        /// <param name="sprite"></param>
        private void OnVisualLoaded(Sprite sprite)
        {
            if (sprite != null)
            {
                currentCard.InitVisual(sprite);
                SetCardVisualAndInformation();

                StartCoroutine(CardAppear());
            }
            else
            {
                Debug.LogError("Failed to load visual");
            }
        }

        /// <summary>
        /// Make the card appears smoothly
        /// </summary>
        /// <returns></returns>
        private IEnumerator CardAppear()
        {
            float t = 0;
            cardImage.rectTransform.localPosition = cardBasePosition + new Vector2(0, verticalOffset);
            Vector2 startPosition = cardImage.rectTransform.localPosition;
            Color tempColor;

            while (t < appearTime)
            {
                cardImage.rectTransform.localPosition = Vector2.Lerp(startPosition, cardBasePosition, t / appearTime);
                tempColor = cardImage.color;
                tempColor.a = Mathf.Lerp(0, 1, t / appearTime);
                cardImage.color = tempColor;

                t += Time.deltaTime;
                yield return null;
            }

            cardImage.rectTransform.localPosition = cardBasePosition;
            tempColor = cardImage.color;
            tempColor.a = 1;
            cardImage.color = tempColor;
            cardButton.interactable = optionsManager.AreAnswersHiden();
        }

        /// <summary>
        /// Called when audio loaded
        /// </summary>
        /// <param name="audioClip"></param>
        private void OnAudioLoaded(AudioClip audioClip)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                
                if (optionsManager.GetAutoPlay())
                {
                    pauseButton.SetIconSprite(pauseSprite);
                    audioSource.Play();
                    pause = false;
                }
                else
                {
                    pauseButton.SetIconSprite(playSprite);
                    audioSource.Pause();
                    pause = true;
                }
            }
            else
            {
                Debug.LogError("Failed to load audio clip");
            }
        }

        /// <summary>
        /// Play next card. Remove current if found == true
        /// </summary>
        /// <param name="found"></param>
        public void NextCard(bool found)
        {
            if (found)
            {
                cards.Remove(currentCard);

                if (cards.Count == 0)
                {
                    cardImage.sprite = loadManager.GetDefaultSprite();
                    audioSource.Stop();
                    cardCounter.text = "Cartes restantes: 0";
                    informationTextMesh.text = "";
                    pauseButton.gameObject.SetActive(false);
                    return;
                }

            }
            cardCounter.text = "Cartes restantes: " + cards.Count.ToString();
            PlayCard();
        }
        #endregion Play Card

        #region Card Visual and Information
        /// <summary>
        /// Set visual informations on the scene
        /// </summary>
        private void SetCardVisualAndInformation()
        {
            // Visual
            // Get the hiding visual of the card visual
            Sprite sprite = (optionsManager.AreAnswersHiden() && cardHiden ? hidingSprite : currentCard.GetVisual());

            float scale = Mathf.Min(maxImageSize.x / sprite.textureRect.width, maxImageSize.y / sprite.textureRect.height);
            cardImage.rectTransform.sizeDelta = sprite.textureRect.size;
            cardImage.rectTransform.localScale = new Vector2(scale, scale);
            cardImage.sprite = sprite;

            // Information text
            informationTextMesh.text = (optionsManager.AreAnswersHiden() && cardHiden ? "???\n???\n???" : currentCard.GetAnime() + "\n" + currentCard.GetCardType() + "\nDeck " + currentCard.GetDeck());
        }

        /// <summary>
        /// Hide or reveal a card
        /// </summary>
        public void ChangeCardState()
        {
            if (cardMoving) { return; }

            cardHiden = !cardHiden;
            SetCardVisualAndInformation();
        }
        #endregion Card Visual and Information

        #region Card Movement
        /// <summary>
        /// Move the card following the position on touchscreen
        /// </summary>
        /// <param name="context"></param>
        public void MoveCard(InputAction.CallbackContext context)
        {
            if (optionsManager.AreAnswersHiden() && cardHiden) { return; }

            TouchState state = context.ReadValue<TouchState>();

            if (state.isInProgress)
            {
                if (!cardMoving && gamePanel.activeSelf)
                {
                    // If the card is not moving, check if the card is touched
                    Vector3 visualCardPosition = (Vector2)cardImage.rectTransform.position - cardImage.rectTransform.pivot * cardImage.rectTransform.sizeDelta * cardImage.rectTransform.lossyScale;

                    if (state.position.x < visualCardPosition.x ||
                        state.position.x > visualCardPosition.x + cardImage.rectTransform.lossyScale.x * cardImage.rectTransform.sizeDelta.x ||
                        state.position.y < visualCardPosition.y ||
                        state.position.y > visualCardPosition.y + cardImage.rectTransform.lossyScale.y * cardImage.rectTransform.sizeDelta.y)
                    {
                        return;
                    }

                    // If true
                    cardMoving = true;
                }
                // Make the card follow verticaly the position on touchscreen
                cardImage.transform.localPosition = cardBasePosition + new Vector2(0, state.position.y - state.startPosition.y);

                int sign = state.position.x - state.startPosition.x < 0 ? 1 : -1;
                cardImage.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(0, sign * maxRotation, (Mathf.Abs(state.position.x - state.startPosition.x)) / rotationDivisor));

                // Set indication color
                Color tempFoundColor = foundColor;
                tempFoundColor.a = Mathf.Lerp(0, 1, (state.position.x - state.startPosition.x) / indicationsFadeDivisor);
                foundColor = tempFoundColor;

                Color tempNotFoundColor = notFoundColor;
                tempNotFoundColor.a = Mathf.Lerp(0, 1, -(state.position.x - state.startPosition.x) / indicationsFadeDivisor);
                notFoundColor = tempNotFoundColor;

                ApplyIndicationColors();
            }
            else
            {
                if (cardMoving)
                {
                    // If validate
                    if (Mathf.Abs(cardImage.transform.localEulerAngles.z - (cardImage.transform.localEulerAngles.z > 90 ? 360 : 0)) > angleThreshold)
                    {
                        // Set indication color to transparent
                        Color tempColor = foundColor;
                        tempColor.a = 0;
                        foundColor = tempColor;

                        tempColor = notFoundColor;
                        tempColor.a = 0;
                        notFoundColor = tempColor;

                        ApplyIndicationColors();

                        cardImage.rectTransform.localPosition = cardBasePosition;
                        cardImage.transform.localEulerAngles = Vector3.zero;

                        cardMoving = false;

                        NextCard(state.position.x - state.startPosition.x > 0);
                    }
                    else
                    {
                        StartCoroutine(ReturnToPosition());
                    }
                }
            }
        }

        /// <summary>
        /// Apply the color correction to the indications
        /// </summary>
        private void ApplyIndicationColors()
        {
            foundImage.color = foundColor;
            foundText.color = foundColor;
            notFoundImage.color = notFoundColor;
            notFoundText.color = notFoundColor;
        }

        /// <summary>
        /// Return card to base position
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReturnToPosition()
        {
            float t = 0;
            Vector2 currentPosition = cardImage.rectTransform.localPosition;
            Vector3 currentEuler = cardImage.rectTransform.localEulerAngles;
            currentEuler.z -= (currentEuler.z > 90 ? 360 : 0);

            Color tempColor;
            float current_a = foundColor.a;
            float current_not_a = notFoundColor.a;

            while (t < returnToPositionTime)
            {
                cardImage.rectTransform.localPosition = Vector2.Lerp(currentPosition, cardBasePosition, t / returnToPositionTime);
                cardImage.rectTransform.localEulerAngles = Vector3.LerpUnclamped(currentEuler, Vector3.zero, t / returnToPositionTime);

                tempColor = foundColor;
                tempColor.a = Mathf.Lerp(current_a, 0, t / returnToPositionTime);
                foundColor = tempColor;

                tempColor = notFoundColor;
                tempColor.a = Mathf.Lerp(current_not_a, 0, t / returnToPositionTime);
                notFoundColor = tempColor;

                ApplyIndicationColors();

                t += Time.deltaTime;
                yield return null;
            }

            cardImage.rectTransform.localPosition = cardBasePosition;
            cardImage.rectTransform.localEulerAngles = Vector3.zero;

            // Set indication color to transparent
            tempColor = foundColor;
            tempColor.a = 0;
            foundColor = tempColor;

            tempColor = notFoundColor;
            tempColor.a = 0;
            notFoundColor = tempColor;

            ApplyIndicationColors();

            Debug.Log("Return to position");

            cardMoving = false;
        }
        #endregion Card Movement

        /// <summary>
        /// Enable / Disable pause
        /// </summary>
        public void SwitchPause()
        {
            pause = !pause;
            if (pause)
            {
                pauseButton.SetIconSprite(playSprite);
                audioSource.Pause();
            }
            else
            {
                pauseButton.SetIconSprite(pauseSprite);
                audioSource.Play();
            }
        }

        /// <summary>
        /// Activate / Desactivate the button to hide answers
        /// </summary>
        private void UpdateHidenProperty()
        {
            cardButton.interactable = optionsManager.AreAnswersHiden();

            SetCardVisualAndInformation();
        }

        private void OnValidate()
        {
            ApplyIndicationColors();
        }
    }
}