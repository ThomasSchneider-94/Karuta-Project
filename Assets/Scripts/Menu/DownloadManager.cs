using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Karuta.ScriptableObjects;
using static Karuta.ScriptableObjects.JsonObjects;
using Karuta.UIComponent;
using System.Text;

namespace Karuta.Menu
{
    public class DownloadManager : MonoBehaviour
    {
        private GameManager gameManager;

        [Header("Debug")]
        [SerializeField] private TextAsset testDeck;

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
         */

        private JsonObjects.JsonDeckInfoList jsonDeckInfoList;
        private string[] visualFiles;
        private string[] soundFiles;

        private void Awake()
        {
            gameManager = GameManager.Instance;

            // GameManager Events
            gameManager.UpdateCategoryEvent.AddListener(HideNonUsedToggles);
            gameManager.UpdateDeckListEvent.AddListener(UpdateDeckDownloadToggles);
            Debug.Log("Dowload Subscription");

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
            List<TextAsset> textAssets = DownloadDeckList();

            SaveDecks(textAssets);

            gameManager.UpdateDeckList();
        }

        /// <summary>
        /// Download the deck list
        /// </summary>
        /// <returns></returns>
        private List<TextAsset> DownloadDeckList()
        {


            //UnityWebRequest request = UnityWebRequest.Get("/deck_names");











            // TODO: Function where you download all the decks file
            return new List<TextAsset>() { testDeck };
        }

        /// <summary>
        /// Save a deck list into the persistant files
        /// </summary>
        /// <param name="textAssets"></param>
        private void SaveDecks(List<TextAsset> textAssets)
        {
            jsonDeckInfoList.deckInfoList.Clear();
            visualFiles = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Visuals"));
            soundFiles = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Sounds"));

            foreach (TextAsset jsonDeck in textAssets) 
            {
                SaveDeck(jsonDeck);
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, "DecksInfo.json"), JsonUtility.ToJson(jsonDeckInfoList));
        }

        /// <summary>
        /// Save a deck into the persistant files
        /// </summary>
        /// <param name="textDeck"></param>
        private void SaveDeck(TextAsset textDeck)
        {
            JsonDeck downloadDeck = JsonUtility.FromJson<JsonDeck>(textDeck.text);

            if (downloadDeck == null) { return; }

            downloadDeck.category = downloadDeck.category.ToUpper();
            downloadDeck.type = downloadDeck.type.ToUpper();

            // Check deck validity
            if (!IsDeckValid(downloadDeck)) { return; }

            // Create Deck Info Object
            var jsonDeckInfo = new JsonDeckInfo
            {
                name = downloadDeck.name,
                category = (int)(DeckInfo.DeckCategory)System.Enum.Parse(typeof(DeckInfo.DeckCategory), downloadDeck.category),
                type = (int)(DeckInfo.DeckType)System.Enum.Parse(typeof(DeckInfo.DeckType), downloadDeck.type),
                cover = downloadDeck.cover,
                isDownloaded = IsDeckAlreadyDownloaded(downloadDeck.cards)
            };

            // Add it to the deck list
            jsonDeckInfoList.deckInfoList.Add(jsonDeckInfo);

            // Create Cards Object
            JsonCards jsonCards = new() { cards = downloadDeck.cards };

            // Save Cards List
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Decks", downloadDeck.category)))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Decks", downloadDeck.category));
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, "Decks", downloadDeck.category, downloadDeck.name + ".json"), JsonUtility.ToJson(jsonCards));
        }

        /// <summary>
        /// Check if deck is valid (do not check the number of cards)
        /// </summary>
        /// <param name="downloadDeck"></param>
        /// <returns></returns>
        private bool IsDeckValid(JsonDeck downloadDeck)
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
            foreach(JsonCard downloadCard in downloadDeck.cards)
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

        /// <summary>
        /// Check if the decks cards are already downloaded in the system
        /// </summary>
        /// <param name="downloadCards"></param>
        /// <returns></returns>
        private bool IsDeckAlreadyDownloaded(List<JsonCard> downloadCards)
        {
            foreach (JsonCard downloadCard in downloadCards)
            {
                if (!Array.Exists(visualFiles, visualFile => visualFile == Path.Combine(Application.persistentDataPath, "Visuals", downloadCard.visual + ".png"))
                    && !Array.Exists(visualFiles, visualFile => visualFile == Path.Combine(Application.persistentDataPath, "Visuals", downloadCard.visual + ".jpg")))
                {
                    Debug.Log("D_ Visual " + downloadCard.visual + " for " + downloadCard.anime + " does not exist");
                    return false;
                }

                if (!Array.Exists(soundFiles, soundFile => soundFile == Path.Combine(Application.persistentDataPath, "Sounds", downloadCard.sound + ".mp3"))
                    && !Array.Exists(soundFiles, soundFile => soundFile == Path.Combine(Application.persistentDataPath, "Sounds", downloadCard.sound + ".wav")))
                {
                    Debug.Log("D_ Sound for " + downloadCard.anime + " does not exist");
                    return false;
                }
            }
            return true;
        }
        #endregion Update Deck list

        /* ======================================== Download Deck Content ======================================== */

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




        public void TestAdd()
        {
            SaveDecks(new List<TextAsset>() { testDeck });
            gameManager.UpdateDeckList();
        }
    }
}