using Karuta.UIComponent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Karuta.Objects;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Collections;
using Unity.VisualScripting;
using System.Security;
using UnityEngine.Events;

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
        [SerializeField] private float sensitivity;
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
        private float distanceTreshold;

        [Header("Indications")]
        [SerializeField] private MultiLayerButton notFoundArrow;
        [SerializeField] private MultiLayerButton foundArrow;

        private readonly List<Card> cards = new();
        private Card currentCard;
        private Vector2 cardBasePosition;
        private bool cardMoving = false;
        private bool cardPressed = false;

        private bool visualLoaded = false;
        private bool audioLoaded = false;

        // Events for Arduino Handler
        public UnityEvent<float, float> ColorIndicationUpdate { get; } = new ();

        private void OnEnable()
        {
            loadManager = LoadManager.Instance;
            optionsManager = OptionsManager.Instance;

            optionsManager.UpdateHidenAnswerEvent.AddListener(UpdateHidenProperty);

            maxImageSize = cardImage.rectTransform.sizeDelta;
            cardBasePosition = cardImage.rectTransform.localPosition;

            ApplyArrowsColors(0, 0);

            distanceTreshold = rotationDivisor * (angleThreshold / maxRotation);

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

                JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategoryName(deck.GetCategory()), deck.GetName() + ".json")));

                foreach (JsonCard jsonCard in jsonCards.cards)
                {
                    cards.Add(new(deck.GetName(), jsonCard));
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
            
            audioLoaded = false;

            // Load visual and audio
            Debug.Log("Visual already initialized: " + currentCard.IsVisualInitialized().ToString());
            if (currentCard.IsVisualInitialized())
            {
                visualLoaded = true;
            }
            else
            {
                visualLoaded = false;
                loadManager.LoadVisual(currentCard.GetVisualName(), OnVisualLoaded);
            }

            loadManager.LoadAudio(currentCard.GetAudioName(), OnAudioLoaded);
        }

        /// <summary>
        /// Called when visual loaded
        /// </summary>
        /// <param name="sprite"></param>
        private void OnVisualLoaded(Sprite sprite, bool success)
        {
            currentCard.InitVisual(sprite, success);

            if (audioLoaded)
            {
                CardLoaded();
            }
            else
            {
                visualLoaded = true;
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
            audioSource.clip = audioClip;

            if (visualLoaded)
            {
                CardLoaded();
            }
            else
            {
                audioLoaded = true;
            }
        }
        
        /// <summary>
        /// Called when all the card informations are loaded
        /// </summary>
        private void CardLoaded()
        {
            // Visual
            SetCardVisualAndInformation();
            StartCoroutine(CardAppear());

            // Audio
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
                    cardImage.gameObject.SetActive(false);
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
            if (!optionsManager.AreAnswersHiden()) { return; }

            cardHiden = !cardHiden;
            SetCardVisualAndInformation();
        }
        #endregion Card Visual and Information

        #region Card Movement
        /// <summary>
        /// Move the card following the currentPosition on touchscreen or hide it
        /// </summary>
        /// <param name="context"></param>
        public void CardInteraction(InputAction.CallbackContext context)
        {
            if (!gamePanel.activeSelf) { return; }

            TouchState state = context.ReadValue<TouchState>();

            if (state.isInProgress)
            {
                if (cardMoving)
                {
                    MoveCard(state.startPosition, state.position);
                }
                else if (cardPressed)
                {

                    // Check if card need to be moved
                    if ((!optionsManager.AreAnswersHiden() || !cardHiden) && Mathf.Abs(state.startPosition.x - state.position.x) > sensitivity)
                    {
                        cardMoving = true;
                    }
                }
                else
                {
                    // Check if the touch is in the card
                    cardPressed = CheckInsideCard(state.startPosition);
                }
            }
            else
            {
                if (cardMoving)
                {
                    // If the card has moved
                    OnMoveRelease(state.startPosition, state.position);
                }
                else if (cardPressed && CheckInsideCard(state.position))
                {
                    Debug.Log("Hide");
                    ChangeCardState();
                }

                cardPressed = false;
                cardMoving = false;
            }
        }

        /// <summary>
        /// Chech if the position is inside the visual card
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        private bool CheckInsideCard(Vector2 currentPosition)
        {
            // If the card is not moving, check if the card is touched
            Vector3 visualCardPosition = (Vector2)cardImage.rectTransform.position - cardImage.rectTransform.pivot * cardImage.rectTransform.sizeDelta * cardImage.rectTransform.lossyScale;

            return  currentPosition.x > visualCardPosition.x &&
                    currentPosition.x < visualCardPosition.x + cardImage.rectTransform.lossyScale.x * cardImage.rectTransform.sizeDelta.x &&
                    currentPosition.y > visualCardPosition.y &&
                    currentPosition.y < visualCardPosition.y + cardImage.rectTransform.lossyScale.y * cardImage.rectTransform.sizeDelta.y;
        }

        /// <summary>
        /// Move the card so it follows the finger
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="currentPosition"></param>
        public void MoveCard(Vector2 startPosition, Vector2 currentPosition)
        {
            // Make the card follow verticaly the currentPosition on touchscreen
            cardImage.transform.localPosition = cardBasePosition + new Vector2(0, currentPosition.y - startPosition.y);

            int sign = currentPosition.x - startPosition.x < 0 ? 1 : -1;
            cardImage.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(0, sign * maxRotation, (Mathf.Abs(currentPosition.x - startPosition.x)) / rotationDivisor));

            ApplyArrowsColors(Mathf.Lerp(0, 1, -(currentPosition.x - startPosition.x) / distanceTreshold),
                Mathf.Lerp(0, 1, (currentPosition.x - startPosition.x) / distanceTreshold));
        }

        /// <summary>
        /// Do an action when the touch is released
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="currentPosition"></param>
        public void OnMoveRelease(Vector2 startPosition, Vector2 currentPosition)
        {
            // If the answer validate, we play the next card
            if (Mathf.Abs(cardImage.transform.localEulerAngles.z - (cardImage.transform.localEulerAngles.z > 90 ? 360 : 0)) > angleThreshold)
            {
                ApplyArrowsColors(0, 0);

                cardImage.rectTransform.localPosition = cardBasePosition;
                cardImage.transform.localEulerAngles = Vector3.zero;

                cardMoving = false;

                NextCard(currentPosition.x - startPosition.x > 0);
            }
            else
            {
                // Else we return to the starting position
                StartCoroutine(ReturnToPosition());
            }
        }

        /// <summary>
        /// Apply the color correction to the indications
        /// </summary>
        private void ApplyArrowsColors(float notFoundColorA, float foundColorA)
        {
            Color tmpColor = notFoundArrow.GetColor(0);
            tmpColor.a = notFoundColorA;
            notFoundArrow.SetColor(0, tmpColor);
            notFoundArrow.GetLabel().color = tmpColor;

            tmpColor = notFoundArrow.GetColor(1);
            tmpColor.a = notFoundColorA;
            notFoundArrow.SetColor(1, tmpColor);

            tmpColor = foundArrow.GetColor(0);
            tmpColor.a = foundColorA;
            foundArrow.SetColor(0, tmpColor);
            foundArrow.GetLabel().color = tmpColor;

            tmpColor = foundArrow.GetColor(1);
            tmpColor.a = foundColorA;
            foundArrow.SetColor(1, tmpColor);

            ColorIndicationUpdate.Invoke(notFoundColorA, foundColorA);
        }

        /// <summary>
        /// Return card to base currentPosition
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReturnToPosition()
        {
            float t = 0;
            Vector2 currentPosition = cardImage.rectTransform.localPosition;
            Vector3 currentEuler = cardImage.rectTransform.localEulerAngles;
            currentEuler.z -= (currentEuler.z > 90 ? 360 : 0);

            float initFoundColorA = foundArrow.GetColor(0).a;
            float initNotFoundColorA = notFoundArrow.GetColor(0).a;

            while (t < returnToPositionTime)
            {
                cardImage.rectTransform.localPosition = Vector2.Lerp(currentPosition, cardBasePosition, t / returnToPositionTime);
                cardImage.rectTransform.localEulerAngles = Vector3.LerpUnclamped(currentEuler, Vector3.zero, t / returnToPositionTime);

                ApplyArrowsColors(Mathf.Lerp(initNotFoundColorA, 0, -t / returnToPositionTime),
                    Mathf.Lerp(initFoundColorA, 0, t / returnToPositionTime));

                t += Time.deltaTime;
                yield return null;
            }

            cardImage.rectTransform.localPosition = cardBasePosition;
            cardImage.rectTransform.localEulerAngles = Vector3.zero;

            ApplyArrowsColors(0, 0);
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyArrowsColors(1, 1);
            distanceTreshold = rotationDivisor * (angleThreshold / maxRotation);
        }
#endif
    }
}