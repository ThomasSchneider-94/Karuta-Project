using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Karuta.ScriptableObjects;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using Karuta.UIComponent;
using static Karuta.ScriptableObjects.JsonObjects;

namespace Karuta.Menu
{
    public class DownloadManager : MonoBehaviour
    {
        private GameManager gameManager;

        [Header("Download Toggles")]
        [SerializeField] private Toggle selectAllToggle;
        [SerializeField] private DeckDownloadToggle deckDownloadTogglePrefab;
        [SerializeField] private VerticalLayoutGroup deckTogglesParent;
        private readonly List<DeckDownloadToggle> deckToggles = new ();

        [Header("Waiting Screen")]
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private TextMeshProUGUI waitingTextMesh;

        /* TODO :
         *      - Download visuals / sounds
         *      - Download and check cover
         *      - Waiting Screen
         */

        private JsonObjects.JsonDeckInfoList jsonDeckInfoList;
        private string[] visualFiles;
        private string[] audioFiles;

        // Variable to pass data for Coroutine
        private List<string> deckNames;
        private string deckInformation;

        private void Awake()
        {
            gameManager = GameManager.Instance;

            // GameManager Events
            gameManager.InitializeDeckListEvent.AddListener(CreateDownloadToggles);
            gameManager.UpdateDeckListEvent.AddListener(UpdateDeckDownloadToggles);
            gameManager.UpdateCategoryEvent.AddListener(HideNonUsedToggles);
            //Debug.Log("Dowload Subscription")

            waitingPanel.SetActive(false);
            
            jsonDeckInfoList = new JsonObjects.JsonDeckInfoList
            {
                deckInfoList = new List<JsonDeckInfo>()
            };

            selectAllToggle.SetIsOnWithoutNotify(false);

            InitDirectories();
        }

        private static void InitDirectories()
        {
            if (!Directory.Exists(GameManager.decksDirectoryPath))
            {
                Directory.CreateDirectory(GameManager.decksDirectoryPath);
            }
            if (!Directory.Exists(GameManager.coversDirectoryPath))
            {
                Directory.CreateDirectory(GameManager.coversDirectoryPath);
            }
            if (!Directory.Exists(GameManager.visualsDirectoryPath))
            {
                Directory.CreateDirectory(GameManager.visualsDirectoryPath);
            }
            if (!Directory.Exists(GameManager.audioDirectoryPath))
            {
                Directory.CreateDirectory(GameManager.audioDirectoryPath);
            }
            if (!Directory.Exists(GameManager.themesDirectoryPath))
            {
                Directory.CreateDirectory(GameManager.themesDirectoryPath);
            }
        }

        /* ======================================== UPDATE THE DECK LIST ======================================== */

        #region Update Deck list
        /// <summary>
        /// Download the deck list and update the local one
        /// </summary>
        public void UpdateDeckList()
        {
            StartCoroutine(UpdateDeckListCoroutine());
        }

        public IEnumerator UpdateDeckListCoroutine()
        {
            waitingPanel.SetActive(true);
            waitingTextMesh.text = "Updating decks";

            jsonDeckInfoList.deckInfoList.Clear();
            visualFiles = Directory.GetFiles(GameManager.visualsDirectoryPath);
            audioFiles = Directory.GetFiles(GameManager.audioDirectoryPath);

            string serverIP = JsonUtility.FromJson<ConfigData>(Resources.Load<TextAsset>("config").text).serverIP;

            // Download the list of decks name
            yield return StartCoroutine(DownloadDeckList(serverIP));

            for (int i = 0; i < deckNames.Count; i++)
            {
                waitingTextMesh.text = "(" + i + "/" + deckNames.Count +")\n" + deckNames[i];
                yield return StartCoroutine(DownloadDeckInformation(serverIP, deckNames[i]));

                SaveDeck(deckInformation);
            }
            
            waitingTextMesh.text = "Saving decks";
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "DecksInfo.json"), JsonUtility.ToJson(jsonDeckInfoList));

            gameManager.UpdateDeckList();

            waitingPanel.SetActive(false);
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
                deckNames = new List<string>(webRequest.downloadHandler.text.Split("\n", StringSplitOptions.RemoveEmptyEntries));
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
                deckInformation = webRequest.downloadHandler.text;
            }
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
                bool isAudioDownloaded = IsCardAudioAlreadyDownloaded(downloadCard.audio);

                jsonCards.Add(new JsonCard
                {
                    anime = downloadCard.anime,
                    type = downloadCard.type,
                    visual = downloadCard.visual,
                    isVisualDownloaded = isVisualDownloaded,
                    audio = downloadCard.audio,
                    isAudioDownloaded = isAudioDownloaded
                });

                if (!isVisualDownloaded || !isAudioDownloaded)
                {
                    isDeckDowloaded = false;
                }
            }

            JsonCards jsonCardS = new() { cards = jsonCards };

            // Save Cards List
            if (!Directory.Exists(Path.Combine(GameManager.decksDirectoryPath, downloadDeck.category)))
            {
                // If the category repertory for the deck does not exist, create it
                Directory.CreateDirectory(Path.Combine(GameManager.decksDirectoryPath, downloadDeck.category));
            }

            File.WriteAllText(Path.Combine(GameManager.decksDirectoryPath, downloadDeck.category, downloadDeck.name + ".json"), JsonUtility.ToJson(jsonCardS));

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
        /// Check if deck is valid (do not check the number of jsonCards)
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
            if (!Array.Exists(visualFiles, visualFile => visualFile == Path.Combine(GameManager.visualsDirectoryPath, downloadCardVisual)))
            {
                //Debug.Log("D_ Visual " + downloadCardVisual + " does not exist")
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the audio file is already downloaded in the system
        /// </summary>
        /// <param name="downloadCardAudio"></param>
        /// <returns></returns>
        private bool IsCardAudioAlreadyDownloaded(string downloadCardAudio)
        {
            if (!Array.Exists(audioFiles, audioFile => audioFile == Path.Combine(GameManager.audioDirectoryPath, downloadCardAudio)))
            {
                //Debug.Log("D_ Audio " + downloadCardAudio + " does not exist")
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
        /// <summary>
        /// Download the content of the decks selectionned
        /// </summary>
        public void DownloadSelectioned()
        {
            StartCoroutine(DownloadSelectionedCoroutine());
        }

        public IEnumerator DownloadSelectionedCoroutine()
        {
            List<DeckInfo> deckList = gameManager.GetDeckList();
            List<DeckInfo> deckListToDownload = new();

            for (int i = 0; i < deckToggles.Count; i++)
            {
                if (deckToggles[i].IsOn())
                {
                    deckListToDownload.Add(deckList[i]);
                }
            }

            if (deckListToDownload.Count > 0)
            {
                string serverIP = JsonUtility.FromJson<ConfigData>(Resources.Load<TextAsset>("config").text).serverIP;

                foreach (DeckInfo deck in deckListToDownload)
                {
                    Debug.Log(deck.GetName());
                    yield return StartCoroutine(DownloadDeckContent(serverIP, deck));
                }
            }
            // TODO : Deselect Toggles
            // - Put deck isDownload
            // - Check if other deck are now partially downloaded

        }

        private IEnumerator DownloadDeckContent(string serverIP, DeckInfo deckInfo)
        {
            string filePath = Path.Combine(GameManager.decksDirectoryPath, deckInfo.GetCategory().ToString(), deckInfo.GetName(), ".json");

            // Vérifier si le fichier existe
            if (File.Exists(filePath))
            {
                JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(filePath));

                foreach (JsonCard jsonCard in jsonCards.cards)
                {
                    if (!jsonCard.isVisualDownloaded)
                    {
                        yield return StartCoroutine(DownloadCardVisual(serverIP, jsonCard.visual));
                        jsonCard.isVisualDownloaded = true;
                    }
                    if (!jsonCard.isAudioDownloaded)
                    {
                        yield return StartCoroutine(DownloadCardAudio(serverIP, jsonCard.audio));
                        jsonCard.isAudioDownloaded = true;
                    }
                }

                File.WriteAllText(Path.Combine(GameManager.decksDirectoryPath, deckInfo.GetCategory().ToString(), deckInfo.GetName()), JsonUtility.ToJson(jsonCards));
            }
        }

        private static IEnumerator DownloadCardVisual(string serverIP, string visual)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(serverIP + "visual/" + visual);

            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(GameManager.visualsDirectoryPath, visual));

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                Debug.Log("File successfully downloaded and saved to " + Path.Combine(GameManager.visualsDirectoryPath, visual));
            }
        }

        private static IEnumerator DownloadCardAudio(string serverIP, string audio)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(serverIP + "sound/" + audio);

            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(GameManager.audioDirectoryPath, audio));

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                Debug.Log("File successfully downloaded and saved to " + Path.Combine(GameManager.audioDirectoryPath, audio));
            }
        }
        #endregion Download

        #endregion Download Deck
    }
}