using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Karuta.ScriptableObjects;
using Karuta.UIComponent;
using System.Text;
using System.Collections;
using UnityEngine.Networking;
using static Karuta.ScriptableObjects.JsonObjects;
using Unity.VisualScripting.FullSerializer;
using System.Net;

namespace Karuta.Menu
{
    public class DownloadManager : MonoBehaviour
    {

        private GameManager gameManager;

        [Header("Decks Download")]
        [SerializeField] private Toggle selectAllToggle;
        [SerializeField] private DeckDownloadToggle deckDownloadTogglePrefab;
        [SerializeField] private VerticalLayoutGroup deckTogglesParent;
        [SerializeField] private float toggleSize;
        [SerializeField] private float toggleSpace;
        private readonly List<DeckDownloadToggle> deckToggles = new ();

        /* TODO :
         *      - Download deck information
         *      - Download visuals / sounds
         *      - Download and check cover
         *      - Waiting Screen
         */

        private JsonObjects.JsonDeckInfoList jsonDeckInfoList;
        private string[] visualFiles;
        private string[] soundFiles;

        private List<string> deckNames;
        private List<string> deckInformations;

        private void Awake()
        {
            gameManager = GameManager.Instance;

            // GameManager Events
            gameManager.InitializeDeckListEvent.AddListener(CreateDownloadToggles);
            gameManager.UpdateDeckListEvent.AddListener(UpdateDeckDownloadToggles);
            gameManager.UpdateCategoryEvent.AddListener(HideNonUsedToggles);
            //Debug.Log("Dowload Subscription")

            jsonDeckInfoList = new JsonObjects.JsonDeckInfoList
            {
                deckInfoList = new List<JsonDeckInfo>()
            };

            selectAllToggle.SetIsOnWithoutNotify(false);

            InitDirectories();
        }

        private static void InitDirectories()
        {
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Decks")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Decks"));
            }
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Covers")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Covers"));
            }
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Sounds")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Sounds"));
            }
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Themes")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Themes"));
            }
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Visuals")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Visuals"));
            }
        }

        /* ======================================== UPDATE THE DECK LIST ======================================== */

        #region Update Deck list
        /// <summary>
        /// Download the deck list and update the local one
        /// </summary>
        public void UpdateDeckList()
        {
            string serverIP = JsonUtility.FromJson<ConfigData>(Resources.Load<TextAsset>("config").text).serverIP;

            StartCoroutine(UpdateDeckListCoroutine(serverIP));
        }

        public IEnumerator UpdateDeckListCoroutine(string serverIP)
        {
            yield return StartCoroutine(DownloadDeckList(serverIP));

            deckInformations = new();

            foreach (string deckName in deckNames)
            {
                if (deckName != null && deckName != "")
                {
                    Debug.Log(deckName);
                    yield return StartCoroutine(DownloadDeckInformation(serverIP, deckName));
                }
            }

            SaveDecks();

            gameManager.UpdateDeckList();
        }

        /// <summary>
        /// Dowload the name of all the decks
        /// </summary>
        /// <param name="serverIP"></param>
        /// <returns></returns>
        private IEnumerator DownloadDeckList(string serverIP)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(serverIP + "deck_names");
            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                // Process received data
                deckNames = new List<string>(webRequest.downloadHandler.text.Split("\n"));
            }
        }

        /// <summary>
        /// Download the information of a deck from its name
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="deckName"></param>
        /// <returns></returns>
        private IEnumerator DownloadDeckInformation(string serverIP, string deckName)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(serverIP + "deck_metadata/" + deckName);

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                // Process received data
                Debug.Log("Response: " + webRequest.downloadHandler.text);
                deckInformations.Add(webRequest.downloadHandler.text);
            }
        }

        /// <summary>
        /// Save a deck list into the persistant files
        /// </summary>
        /// <param name="textAssets"></param>
        private void SaveDecks()
        {
            jsonDeckInfoList.deckInfoList.Clear();
            visualFiles = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Visuals"));
            soundFiles = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Sounds"));

            foreach (string jsonDeck in deckInformations) 
            {
                SaveDeck(jsonDeck);
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, "DecksInfo.json"), JsonUtility.ToJson(jsonDeckInfoList));
        }

        /// <summary>
        /// Save a deck into the persistant files
        /// </summary>
        /// <param name="textDeck"></param>
        private void SaveDeck(string textDeck)
        {
            DownloadDeck downloadDeck = JsonUtility.FromJson<DownloadDeck>(textDeck);

            if (downloadDeck == null) { return; }

            downloadDeck.category = downloadDeck.category.ToUpper();
            downloadDeck.type = downloadDeck.type.ToUpper();

            // Check deck validity
            if (!IsDeckValid(downloadDeck)) { return; }

            // Create Cards Object
            List<JsonCard> jsonCards = new();
            bool isDeckDowloaded = true;

            foreach (DownloadCard downloadCard in downloadDeck.cards)
            {
                bool isVisualDownloaded = IsCardVisualAlreadyDownloaded(downloadCard.visual);
                bool isSoundDownloaded = IsCardAudioAlreadyDownloaded(downloadCard.audio);

                jsonCards.Add(new JsonCard
                {
                    anime = downloadCard.anime,
                    type = downloadCard.type,
                    visual = downloadCard.visual,
                    isVisualDownloaded = isVisualDownloaded,
                    audio = downloadCard.audio,
                    isAudioDownloaded = isSoundDownloaded
                });

                if (!isVisualDownloaded || !isSoundDownloaded)
                {
                    isDeckDowloaded = false;
                }
            }

            JsonCards jsonCardS = new() { cards = jsonCards };

            // Save Cards List
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Decks", downloadDeck.category)))
            {
                // If the category repertory for the deck does not exist, create it
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Decks", downloadDeck.category));
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, "Decks", downloadDeck.category, downloadDeck.name + ".json"), JsonUtility.ToJson(jsonCardS));

            // Create Deck Info Object
            var jsonDeckInfo = new JsonDeckInfo
            {
                name = downloadDeck.name,
                category = (int)(DeckInfo.DeckCategory)System.Enum.Parse(typeof(DeckInfo.DeckCategory), downloadDeck.category),
                type = (int)(DeckInfo.DeckType)System.Enum.Parse(typeof(DeckInfo.DeckType), downloadDeck.type),
                cover = downloadDeck.cover,
                isDownloaded = isDeckDowloaded
            };

            // Add it to the deck list
            jsonDeckInfoList.deckInfoList.Add(jsonDeckInfo);
        }

        #region Check Deck Validity
        /// <summary>
        /// Check if deck is valid (do not check the number of cards)
        /// </summary>
        /// <param name="downloadDeck"></param>
        /// <returns></returns>
        private bool IsDeckValid(DownloadDeck downloadDeck)
        {
            // Check Deck Information
            if (downloadDeck.name == null || downloadDeck.category == null || downloadDeck.type == null || downloadDeck.cover == null && downloadDeck.cards == null) // All attributes are not null
            {
                Debug.Log("D_ " + string.Format("Deck {0} has null attributes: category: {1}; type: {2}; cover {3}", downloadDeck.name, downloadDeck.category, downloadDeck.type, downloadDeck.cover));
                return false;
            }
            if (!IsEnumValue<DeckInfo.DeckCategory>(downloadDeck.category) || downloadDeck.category == DeckInfo.DeckCategory.CATEGORY_NB.ToString()) // Category
            {
                Debug.Log("D_ " + string.Format("Deck {0} category do not match Enum: {1}", downloadDeck.name, downloadDeck.category));
                return false; 
            }
            if (!IsEnumValue<DeckInfo.DeckType>(downloadDeck.type) || downloadDeck.type == DeckInfo.DeckType.TYPE_NB.ToString()) // Type
            {
                Debug.Log("D_ " + string.Format("Deck {0} type do not match Enum: {1}", downloadDeck.name, downloadDeck.type));
                return false; 
            } 
            if (DeckAlreadyExist(downloadDeck.name, (int)(DeckInfo.DeckCategory)System.Enum.Parse(typeof(DeckInfo.DeckCategory), downloadDeck.category))) // Name does not already exist
            {
                Debug.Log("D_ " + string.Format("Deck {0} already exist", downloadDeck.name));
                return false; 
            }

            // Check Cards Information
            foreach(DownloadCard downloadCard in downloadDeck.cards)
            {
                if (downloadCard.anime == null || downloadCard.type == null || downloadCard.visual == null) 
                {
                    Debug.Log("D_ " + string.Format("Card {0} of deck {1} has null attributes: type: {2}; visual {3}", downloadCard.anime, downloadDeck.name, downloadCard.type, downloadCard.visual));
                    return false; 
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the string is part of a given Enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEnumValue<T>(string value) where T : struct
        {
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// Check if the deck does not already exist in the decks previously saved
        /// </summary>
        /// <param name="deckName"></param>
        /// <param name="deckCategory"></param>
        /// <returns></returns>
        private bool DeckAlreadyExist(string deckName, int deckCategory) 
        {
            foreach (JsonDeckInfo deckInfo in jsonDeckInfoList.deckInfoList)
            {
                // A deck already exist when another deck has the same name and category
                if (deckInfo.name == deckName && deckInfo.category == deckCategory)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion Check Deck Validity

        /// <summary>
        /// Check if the visual file is already downloaded in the system
        /// </summary>
        /// <param name="downloadCardVisual"></param>
        /// <returns></returns>
        private bool IsCardVisualAlreadyDownloaded(string downloadCardVisual)
        {
            if (!Array.Exists(visualFiles, visualFile => visualFile == Path.Combine(Application.persistentDataPath, "Visuals", downloadCardVisual + ".png"))
                    && !Array.Exists(visualFiles, visualFile => visualFile == Path.Combine(Application.persistentDataPath, "Visuals", downloadCardVisual + ".jpg")))
            {
                //Debug.Log("D_ Visual " + downloadCardVisual + " does not exist")
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the audio file is already downloaded in the system
        /// </summary>
        /// <param name="downloadCardSound"></param>
        /// <returns></returns>
        private bool IsCardAudioAlreadyDownloaded(string downloadCardSound)
        {
            if (!Array.Exists(soundFiles, soundFile => soundFile == Path.Combine(Application.persistentDataPath, "Sounds", downloadCardSound + ".mp3"))
                    && !Array.Exists(soundFiles, soundFile => soundFile == Path.Combine(Application.persistentDataPath, "Sounds", downloadCardSound + ".wav")))
            {
                //Debug.Log("D_ Sound " + downloadCardSound + " does not exist")
                return false;
            }
            return true;
        }
        #endregion Update Deck list

        /* ======================================== DOWNLOAD DECK CONTENT ======================================== */

        #region Download Deck

        #region Toggles Creation
        /// <summary>
        /// Update the decks download toggles
        /// </summary>
        private void UpdateDeckDownloadToggles()
        {
            //Debug.Log("!D Update Deck Toggles")

            foreach (DeckDownloadToggle deckDownloadToggle in deckToggles)
            {
                GameObject.Destroy(deckDownloadToggle.gameObject);
            }
            deckToggles.Clear();

            CreateDownloadToggles();
        }

        /// <summary>
        /// Create all the Download toggles
        /// </summary>
        private void CreateDownloadToggles()
        {
            //Debug.Log("!D Create Deck Toggles")

            foreach (DeckInfo deck in gameManager.GetDeckList())
            {
                CreateDownloadToggle(deck.GetName());
            }
            HideNonUsedToggles();
        }

        /// <summary>
        /// Create a Download Toggle
        /// </summary>
        /// <param name="deckName"></param>
        private void CreateDownloadToggle(string deckName)
        {
            DeckDownloadToggle deckDownloadToggle = GameObject.Instantiate(deckDownloadTogglePrefab);

            deckDownloadToggle.transform.SetParent(deckTogglesParent.transform); // setting parent

            // Set Toggle Values
            deckDownloadToggle.SetDeckName(deckName);
            deckDownloadToggle.SetIsOnWithoutNotify(false);

            deckDownloadToggle.ChangeToggleValue.AddListener(CheckIfAllToggleSelectioned);

            deckToggles.Add(deckDownloadToggle);
        }
        #endregion Toggles Creation

        #region Toggles Gestion
        /// <summary>
        /// Hide or Show the deck download toggle. Hide if the deck is already downloaded or its category is not the current one
        /// </summary>
        private void HideNonUsedToggles()
        {
            List<DeckInfo> deckList = gameManager.GetDeckList();

            for (int i = 0; i < deckList.Count; i++)
            {
                if (!deckList[i].IsDownloaded() && gameManager.IsCategoryActive(deckList[i].GetCategory()))
                {
                    deckToggles[i].gameObject.SetActive(true);
                }
                else
                {
                    deckToggles[i].gameObject.SetActive(false);
                    deckToggles[i].SetIsOnWithoutNotify(false);
                }
            }
            CheckIfAllToggleSelectioned();
        }

        /// <summary>
        /// Check if all active toggles are on
        /// </summary>
        public void CheckIfAllToggleSelectioned()
        {
            foreach (DeckDownloadToggle deckDownloadToggle in deckToggles)
            {
                if (!deckDownloadToggle.IsOn() && deckDownloadToggle.gameObject.activeSelf)
                {

                    selectAllToggle.SetIsOnWithoutNotify(false);
                    return;
                }
            }
            selectAllToggle.SetIsOnWithoutNotify(true);
        }

        /// <summary>
        /// Set all the active toggles to the value of the SelectAll toggle
        /// </summary>
        public void SetAllToggleIsOn()
        {
            bool isOn = selectAllToggle.isOn;

            foreach(DeckDownloadToggle deckDownloadToggle in deckToggles)
            {
                if (deckDownloadToggle.gameObject.activeSelf)
                {
                    deckDownloadToggle.SetIsOnWithoutNotify(isOn);
                }
            }
        }
        #endregion Toggles Gestion

        #region Download
        // Download the files of the decks selectionned
        public void DownloadSelectioned()
        {
            List<DeckInfo> deckList = gameManager.GetDeckList();
            StringBuilder dump = new();

            for (int i = 0; i < deckToggles.Count; i++)
            {
                if (deckToggles[i].IsOn() && deckToggles[i].gameObject.activeSelf)
                {
                    dump.Append(deckList[i].name + "; ");
                }
            }

            // TODO : Download decks

            Debug.Log("D_ " + dump.ToString());
        }
        #endregion Download

        #endregion Download Deck
    }
}