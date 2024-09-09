using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEditor.PackageManager;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Data;

namespace Karuta
{
    #region Download Json Objects
    [Serializable]
    public class DownloadDeck
    {
        public string name;
        public string category;
        public string type;
        public string cover;
        public List<DownloadCard> cards;
    }

    [Serializable]
    public class DownloadCard
    {
        public string anime;
        public string type;
        public string visual;
        public string sound;
    }
    #endregion Download Json Objects

    public class DownloadManager : MonoBehaviour
    {
        private GameManager gameManager;

        [Header("Debug")]
        [SerializeField] private TextAsset testDeck;

        [Header("Decks Download")]
        [SerializeField] private Toggle selectAllToggle;
        [SerializeField] private DeckDownloadToggle deckDownloadTogglePrefab;
        [SerializeField] private Transform deckTogglesParent;
        [SerializeField] private float toggleSize;
        [SerializeField] private float toggleSpace;
        private List<DeckDownloadToggle> deckToggles;

        /* TODO :
         *      - Download deck information
         *      - Download visuals / sounds
         *      - Download and check cover
         */

        private JsonDeckInfoList jsonDeckInfoList;
        private string[] visualFiles;
        private string[] soundFiles;

        public void Start()
        {
            gameManager = GameManager.Instance;
            jsonDeckInfoList = new JsonDeckInfoList
            {
                deckInfoList = new List<JsonDeckInfo>()
            };
            deckToggles = new List<DeckDownloadToggle>();

            selectAllToggle.SetIsOnWithoutNotify(false);

            InitDirectories();
            CreateDownloadToggles();
        }

        private void InitDirectories()
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

        // Update the list of decks
        #region Update Deck list
        public void UpdateDeckList()
        {
            List<TextAsset> textAssets = DownloadDeckList();

            SaveDecks(textAssets);

            gameManager.RefreshDeckList();
        }

        private List<TextAsset> DownloadDeckList()
        {
            // TODO: Function where you download all the decks file
            return new List<TextAsset>() { testDeck };
        }

        // Save a deck list into the persistant files
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

        // Save a deck into the persistant files
        private void SaveDeck(TextAsset textDeck)
        {
            DownloadDeck downloadDeck = JsonUtility.FromJson<DownloadDeck>(textDeck.text);

            // Check deck validity
            if (downloadDeck == null || !IsDeckValid(downloadDeck)) { return; }

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
            JsonCards jsonCards = new ()
            {
                cards = new List<JsonCard>()
            };
            foreach (DownloadCard downloadCard in downloadDeck.cards)
            {
                var jsonCard = new JsonCard
                {
                    anime = downloadCard.anime,
                    type = downloadCard.type,
                    visual = downloadCard.visual,
                    sound = downloadCard.sound
                };
                jsonCards.cards.Add(jsonCard);
            }

            // Save Cards List
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Decks", downloadDeck.category)))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Decks", downloadDeck.category));
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, "Decks", downloadDeck.category, downloadDeck.name + ".json"), JsonUtility.ToJson(jsonCards));
        }

        // Check if deck is valid (do not check the number of cards)
        private bool IsDeckValid(DownloadDeck downloadDeck)
        {
            // Check Deck Information
            if (downloadDeck.name == null || downloadDeck.category == null || downloadDeck.type == null || downloadDeck.cover == null && downloadDeck.cards == null) // All attributes are not null
            {
                Debug.Log(string.Format("Deck {0} has null attributes: category: {1}; type: {2}; cover {3}", downloadDeck.name, downloadDeck.category, downloadDeck.type, downloadDeck.cover));
                return false;
            }
            if (!IsEnumValue<DeckInfo.DeckCategory>(downloadDeck.category)) // Category
            {
                Debug.Log(string.Format("Deck {0} category do not match Enum: {1}", downloadDeck.name, downloadDeck.category));
                return false; 
            } 
            if (!IsEnumValue<DeckInfo.DeckType>(downloadDeck.type)) // Type
            {
                Debug.Log(string.Format("Deck {0} type do not match Enum: {1}", downloadDeck.name, downloadDeck.type));
                return false; 
            } 
            if (DeckAlreadyExist(downloadDeck.name, (int)(DeckInfo.DeckCategory)System.Enum.Parse(typeof(DeckInfo.DeckCategory), downloadDeck.category))) // Name does not already exist
            {
                Debug.Log(string.Format("Deck {0} already exist", downloadDeck.name));
                return false; 
            }

            // Check Cards Information
            foreach(DownloadCard downloadCard in downloadDeck.cards)
            {
                if (downloadCard.anime == null || downloadCard.type == null || downloadCard.visual == null) 
                {
                    Debug.Log(string.Format("Card {0} of deck {1} has null attributes: type: {2}; visual {3}", downloadCard.anime, downloadDeck.name, downloadCard.type, downloadCard.visual));
                    return false; 
                }
            }

            return true;
        }

        public bool IsEnumValue<T>(string value) where T : struct
        {
            return Enum.IsDefined(typeof(T), value);
        }

        // Check if the deck does not already exist in the decks previously saved
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

        // Check if the decks cards are already downloaded in the system
        private bool IsDeckAlreadyDownloaded(List<DownloadCard> downloadCards)
        {
            foreach (DownloadCard downloadCard in downloadCards)
            {
                if (!Array.Exists(visualFiles, visualFile => visualFile == Path.Combine(Application.persistentDataPath, "Visuals", downloadCard.visual + ".png"))
                    && !Array.Exists(visualFiles, visualFile => visualFile == Path.Combine(Application.persistentDataPath, "Visuals", downloadCard.visual + ".jpg")))
                {
                    Debug.Log("Visual " + downloadCard.visual + " for " + downloadCard.anime + " does not exist");
                    return false;
                }

                if (!Array.Exists(soundFiles, soundFile => soundFile == Path.Combine(Application.persistentDataPath, "Sounds", downloadCard.sound + ".mp3"))
                    && !Array.Exists(soundFiles, soundFile => soundFile == Path.Combine(Application.persistentDataPath, "Sounds", downloadCard.sound + ".wav")))
                {
                    Debug.Log("Sound for " + downloadCard.anime + " does not exist");
                    return false;
                }
            }
            return true;
        }
        #endregion Update Deck list

        // Download Deck Content
        #region Download Deck
        private void CreateDownloadToggles()
        {
            foreach (DeckInfo deck in gameManager.GetDeckList())
            {
                Debug.Log(deck.Dump());
                CreateDownloadToggle(deck.GetName());
            }
            HideShowDeckDownloadToggles();
        }

        private void CreateDownloadToggle(string deckName)
        {
            DeckDownloadToggle deckDownloadToggle = GameObject.Instantiate(deckDownloadTogglePrefab, Vector3.zero, Quaternion.identity);
            //RectTransform toggleTransform = deckDownloadToggle.GetComponent<RectTransform>();

            deckDownloadToggle.transform.SetParent(deckTogglesParent); // setting parent

            // To add a custom size
            //toggleTransform.sizeDelta = new Vector2(toggleSize, toggleSize);
            //toggleTransform.localScale = new Vector2(1, 1);

            // Set Toggle
            deckDownloadToggle.SetDeckName(deckName);
            deckDownloadToggle.SetIsOnWithoutNotify(false);

            deckToggles.Add(deckDownloadToggle);
        }

        public void HideShowDeckDownloadToggles()
        {
            List<DeckInfo> deckList = gameManager.GetDeckList();
            DeckInfo.DeckCategory currentCategory = gameManager.GetCurrentCategory();
            bool differentCategory = gameManager.GetDifferentCategory();

            Debug.Log("Show / Hide");
            for (int i = 0; i < deckList.Count; i++)
            {
                deckToggles[i].gameObject.SetActive(!deckList[i].IsDownloaded() && (differentCategory || currentCategory == deckList[i].GetCategory()));
            }
        }

        public void SetAllToggleIsOn()
        {
            bool isOn = selectAllToggle.isOn;

            foreach(DeckDownloadToggle deckDownloadToggle in deckToggles)
            {
                deckDownloadToggle.SetIsOnWithoutNotify(isOn && deckDownloadToggle.gameObject.activeSelf);
            }
        }


        /*
        private void createButton(float place, int index, Navigation newNav)
        {
            GameObject purchaseButton = GameObject.Instantiate(purchaseButtonPrefab, Vector3.zero, Quaternion.identity);
            RectTransform rectTrans = purchaseButton.GetComponent<RectTransform>();

            purchaseButton.transform.SetParent(purchaseUI.transform); // setting parent
            rectTrans.anchoredPosition = new Vector2(place * (buttonSize + buttonSpacing), 0f); // set position


            // To add a custom size (prefab as normally the good size)
            rectTrans.sizeDelta = new Vector2(buttonBackgroundSize, buttonBackgroundSize);
            rectTrans.localScale = new Vector2(1, 1);
            RectTransform rectTransButton = unitImage.gameObject.GetComponent<RectTransform>();
            rectTransButton.sizeDelta = new Vector2(buttonSize, buttonSize);

            // Add image to the button
            Texture2D tex = Resources.Load<Texture2D>(((Troup.UnitType)index).ToString());

            Image buttonImage = unitImage.gameObject.GetComponent<Image>();
            buttonImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            // Add function to the button
            Button button = purchaseButton.GetComponent<Button>();
            button.onClick.AddListener(delegate { chooseTroup(index); });

            // Get unit informations
            GameObject unitInfo = purchaseButton.transform.GetChild(1).gameObject;
            if (gameManager.getUnitPrefabs()[index - 1] != null)
            {
                Troup unit = gameManager.getUnitPrefabs()[index - 1].GetComponent<Troup>();
                unitInfo.GetComponent<UnitInfo>().completeValues(unit.getCost(), unit.getHealth(),
                                                                 unit.getArmor(), unit.getSpeed(),
                                                                 unit.getAttack(), unit.getAttackSpeed(),
                                                                 unit.getAttackRange(), unit.getAbilityRecharge());
            }
            unitInfo.SetActive(false);

            // Add event to the button
            EventTrigger eventTrigger = purchaseButton.GetComponent<EventTrigger>();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((eventData) => { unitInfo.SetActive(true); });
            eventTrigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((eventData) => { unitInfo.SetActive(false); });
            eventTrigger.triggers.Add(exitEntry);

            //Assign the new navigation to the button
            button.navigation = newNav;
        }*/
        #endregion Download Deck

        public void TestAdd()
        {
            SaveDecks(new List<TextAsset>() { testDeck });
            gameManager.RefreshDeckList();
        }
    }
}