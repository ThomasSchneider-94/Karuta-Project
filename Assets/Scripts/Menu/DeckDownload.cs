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
using System.Threading.Tasks;
using static Karuta.ScriptableObjects.JsonObjects;
using Unity.VisualScripting;

namespace Karuta.Menu
{
    public class DeckDownload : MonoBehaviour
    {
        private LoadManager loadManager;

        [SerializeField] private int downloadTimeout;
        [SerializeField] private Button downloadDeckButton;

        [Header("Download Toggles")]
        [SerializeField] private Toggle selectAllToggle;
        [SerializeField] private LabeledToggle togglePrefab;
        [SerializeField] private VerticalLayoutGroup togglesParent;
        [SerializeField] private float toggleSpacing;
        [SerializeField] private Vector2 toggleScale;
        private readonly List<LabeledToggle> toggles = new ();

        [Header("Waiting Screen")]
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private TextMeshProUGUI downloadingDeckTextMesh1;
        [SerializeField] private TextMeshProUGUI downloadingDeckTextMesh2;

        [Header("Debug")]
        [SerializeField] private bool debugOn;

        /* TODO :
         *      - Download and check cover
         */

        // Files 
        string[] visualFiles;
        string[] audioFiles;

        private void OnEnable()
        {
            loadManager = LoadManager.Instance;

            // LoadManager Events
            DecksManager.Instance.UpdateDeckListEvent.AddListener(UpdateToggles);
            OptionsManager.Instance.UpdateCategoryEvent.AddListener(HideNonUsedToggles);

            waitingPanel.SetActive(false);
            downloadDeckButton.interactable = false;

            selectAllToggle.SetIsOnWithoutNotify(false);

            // Initialize
            if (DecksManager.Instance.IsInitialized())
            {
                Initialize();
            }
            else
            {
                DecksManager.Instance.DeckListInitializedEvent.AddListener(Initialize);
            }
        }

        private void Initialize()
        {
            CreateToggles();
        }

        #region Update Deck list
        public void StartUpdateDeckList()
        {
            StartCoroutine(UpdateDeckList());
        }

        /// <summary>
        /// Download the deck list and update the local one
        /// </summary>
        private IEnumerator UpdateDeckList()
        {
            waitingPanel.SetActive(true);
            downloadingDeckTextMesh1.text = "Updating decks";

            // Download the list of decks name
            List<string> deckNames = new();
            if (!debugOn)
            {
                yield return StartCoroutine(DownloadDeckList(deckNames));
            }
            else
            {
                deckNames = new(Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Debug")));
            }

            if (deckNames.Count == 0) { yield return StartCoroutine(ExitOnDownloadFail("Downloading deck list failed")); }

            // List Init 
            visualFiles = Directory.GetFiles(LoadManager.VisualsDirectoryPath);
            audioFiles = Directory.GetFiles(LoadManager.AudioDirectoryPath);
            JsonDeckInfoList jsonDeckInfoList = new()
            {
                deckInfoList = new()
            };
            List<string> deckContents = new();

            for (int i = 0; i < deckNames.Count; i++)
            {
                downloadingDeckTextMesh1.text = "(" + (i + 1) + "/" + deckNames.Count + ")\n" + deckNames[i];

                yield return StartCoroutine(DownloadDeckInformation(deckNames[i], deckContents));

                if (deckContents[^1] != null)
                {
                    JsonDeckInfo deckInfo = SaveDeck(deckContents[^1]);

                    // Add it to the deck list
                    if (deckInfo != null)
                    {
                        jsonDeckInfoList.deckInfoList.Add(deckInfo);
                    }
                }
            }

            downloadingDeckTextMesh1.text = "Saving decks";
            File.WriteAllText(LoadManager.DecksFilePath, JsonUtility.ToJson(jsonDeckInfoList));

            DecksManager.Instance.UpdateDeckList();

            waitingPanel.SetActive(false);
        }

        /// <summary>
        /// Dowload the name of all the decks
        /// </summary>
        /// <param name="deckNames"></param>
        /// <returns></returns>
        private IEnumerator DownloadDeckList(List<String> deckNames)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "deck_names");
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string[] names = webRequest.downloadHandler.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < names.Length; i++)
                {
                    deckNames.Add(names[i]);
                }
            }
        }

        /// <summary>
        /// Download the information of a deck from its name
        /// </summary>
        /// <param name="deckName"></param>
        /// <param name="deckContents"></param>
        /// <returns></returns>
        private IEnumerator DownloadDeckInformation(string deckName, List<string> deckContents)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "deck_metadata/" + deckName);
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                deckContents.Add(webRequest.downloadHandler.text);
            }
            else
            {
                deckContents.Add(null);
            }
        }

        /// <summary>
        ///  Save a deck  and its cards into the persistant files
        /// </summary>
        /// <param name="textDeck"></param>
        /// <returns></returns>
        private JsonDeckInfo SaveDeck(string textDeck)
        {
            DownloadDeck downloadDeck = JsonUtility.FromJson<DownloadDeck>(textDeck);

            downloadDeck.category = downloadDeck.category.ToUpper();
            downloadDeck.type = downloadDeck.type.ToUpper();

            // Check deck validity
            if (!IsDeckValid(downloadDeck)) { return null; }

            // Save Cards List
            if (!Directory.Exists(Path.Combine(LoadManager.DecksDirectoryPath, downloadDeck.category)))
            {
                // If the category repertory for the deck does not exist, create it
                Directory.CreateDirectory(Path.Combine(LoadManager.DecksDirectoryPath, downloadDeck.category));
            }

            File.WriteAllText(Path.Combine(LoadManager.DecksDirectoryPath, downloadDeck.category, downloadDeck.name + ".json"), JsonUtility.ToJson(new JsonCards() { cards = downloadDeck.cards }));

            // Create Deck Info Object
            return new JsonDeckInfo
            {
                name = downloadDeck.name,
                category = (int)(DeckInfo.DeckCategory)System.Enum.Parse(typeof(DeckInfo.DeckCategory), downloadDeck.category),
                type = (int)(DeckInfo.DeckType)System.Enum.Parse(typeof(DeckInfo.DeckType), downloadDeck.type),
                cover = downloadDeck.cover,
                isDownloaded = IsDeckDownloaded(downloadDeck),
            };
        }
        
        #region Check Deck Validity
        /// <summary>
        /// Check if deck is valid (do not check the number of jsonCards)
        /// </summary>
        /// <param name="downloadDeck"></param>
        /// <returns></returns>
        private static bool IsDeckValid(DownloadDeck downloadDeck)
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

            // Check Cards Information
            foreach(JsonCard jsonCard in downloadDeck.cards)
            {
                if (jsonCard.anime == null || jsonCard.type == null || jsonCard.visual == null) 
                {
                    Debug.Log("D_ " + string.Format("Card {0} of deck {1} has null attributes: type: {2}; visual {3}", jsonCard.anime, downloadDeck.name, jsonCard.type, jsonCard.visual));
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
        #endregion Check Deck Validity

        #endregion Update Deck list

        #region Toggles

        #region Toggles Creation
        /// <summary>
        /// Update the decks download toggles
        /// </summary>
        private void UpdateToggles()
        {
            //Debug.Log("!D Update Deck Toggles")

            foreach (LabeledToggle toggle in toggles)
            {
                GameObject.Destroy(toggle.gameObject);
            }
            toggles.Clear();

            CreateToggles();
        }

        /// <summary>
        /// Create all the toggles to download decks
        /// </summary>
        private void CreateToggles()
        {
            //Debug.Log("!D Create Deck Toggles")
            togglesParent.spacing = toggleSpacing;


            foreach (DeckInfo deck in DecksManager.Instance.GetDeckList())
            {
                CreateToggle(deck.GetName());
            }
            HideNonUsedToggles();
        }

        /// <summary>
        /// Create a toggle to download a deck
        /// </summary>
        /// <param name="deckName"></param>
        private void CreateToggle(string deckName)
        {
            LabeledToggle toggle = GameObject.Instantiate(togglePrefab);

            toggle.transform.SetParent(togglesParent.transform); // setting parent
            toggle.transform.localScale = toggleScale;

            // Set Toggle Values
            toggle.SetLabel(deckName);
            toggle.SetIsOnWithoutNotify(false);

            toggle.onValueChanged.AddListener(delegate { CheckIfAllToggleSelected(); });
            toggle.onValueChanged.AddListener(delegate { CheckIfOneToggleSelected(); });

            toggles.Add(toggle);
        }
        #endregion Toggles Creation

        #region Toggles Gestion
        /// <summary>
        /// Hide or Show the deck toggle. Hide if the deck is already downloaded or its category is not the current one
        /// </summary>
        private void HideNonUsedToggles()
        {
            List<DeckInfo> deckList = DecksManager.Instance.GetDeckList();

            for (int i = 0; i < deckList.Count; i++)
            {
                if (!deckList[i].IsDownloaded() && OptionsManager.Instance.IsCategoryActive(deckList[i].GetCategory()))
                {
                    toggles[i].gameObject.SetActive(true);
                }
                else
                {
                    toggles[i].gameObject.SetActive(false);
                    toggles[i].SetIsOnWithoutNotify(false);
                }
            }
            CheckIfAllToggleSelected();
        }

        /// <summary>
        /// Check if all active toggles are on
        /// </summary>
        private void CheckIfAllToggleSelected()
        {
            foreach (LabeledToggle toggle in toggles)
            {
                if (!toggle.isOn && toggle.gameObject.activeSelf)
                {
                    selectAllToggle.SetIsOnWithoutNotify(false);
                    return;
                }
            }
            selectAllToggle.SetIsOnWithoutNotify(true);
        }

        /// <summary>
        /// Check if a toggle is selected
        /// </summary>
        private void CheckIfOneToggleSelected()
        {
            foreach (LabeledToggle toggle in toggles)
            {
                if (toggle.isOn)
                {
                    downloadDeckButton.interactable = true;
                    return;
                }
            }
            downloadDeckButton.interactable = false;
        }

        /// <summary>
        /// Set all the active toggles to the value of the SelectAll toggle
        /// </summary>
        public void SetAllToggleIsOn()
        {
            bool isOn = selectAllToggle.isOn;

            foreach(LabeledToggle toggle in toggles)
            {
                if (toggle.gameObject.activeSelf)
                {
                    toggle.SetIsOnWithoutNotify(isOn);
                }
            }
            downloadDeckButton.interactable = isOn;
        }
        #endregion Toggles Gestion

        #endregion Toggles

        #region Download Deck Content
        public void StartDownloadSelected()
        {
            StartCoroutine(DownloadSelected());
        }

        /// <summary>
        /// Download the content of the decks selected
        /// </summary>
        private IEnumerator DownloadSelected()
        {
            int downloadTotal = 0;
            foreach (LabeledToggle toggle in toggles)
            {
                if (toggle.isOn)
                {
                    downloadTotal++;
                }
            }
            
            if (downloadTotal > 0)
            {
                waitingPanel.SetActive(true);
                List<bool> downloadDeckSucced = new() { false };

                List<DeckInfo> deckList = DecksManager.Instance.GetDeckList();
                JsonDeckInfoList jsonDeckInfoList = new()
                {
                    deckInfoList = new()
                };
                visualFiles = Directory.GetFiles(LoadManager.VisualsDirectoryPath);
                audioFiles = Directory.GetFiles(LoadManager.AudioDirectoryPath);

                int downloadCount = 0;
                // Download the content of each selected decks
                for (int i = 0; i < deckList.Count; i++)
                {
                    if (toggles[i].isOn)
                    {
                        downloadingDeckTextMesh1.text = "(" + (downloadCount + 1) + "/" + downloadTotal + ") " + deckList[i].GetName();

                        yield return DownloadDeckContent(deckList[i], downloadDeckSucced);

                        if (!downloadDeckSucced[0]) { break; }

                        downloadCount++;
                    }
                }

                // Check if the new download has downloaded another deck
                visualFiles = Directory.GetFiles(LoadManager.VisualsDirectoryPath);
                audioFiles = Directory.GetFiles(LoadManager.AudioDirectoryPath);

                for (int i = 0; i < deckList.Count; i++)
                {
                    downloadingDeckTextMesh1.text = "(" + (i + 1) + "/" + deckList.Count + ") " + deckList[i].GetName();

                    jsonDeckInfoList.deckInfoList.Add(new JsonDeckInfo()
                    {
                        name = deckList[i].GetName(),
                        category = (int)deckList[i].GetCategory(),
                        type = (int)deckList[i].GetDeckType(),
                        cover = deckList[i].GetCoverName(),
                        isDownloaded = deckList[i].IsDownloaded() || IsDeckDownloaded(deckList[i])
                    });
                }

                File.WriteAllText(LoadManager.DecksFilePath, JsonUtility.ToJson(jsonDeckInfoList));

                DecksManager.Instance.UpdateDeckList();

                waitingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Download the visuals and audio of a deck
        /// </summary>
        /// <param name="deckInfo"></param>
        /// <param name="isDeckDownloaded"></param>
        /// <returns></returns>
        private IEnumerator DownloadDeckContent(DeckInfo deckInfo, List<bool> isDeckDownloaded)
        {
            // If the deck file does not exist, do not try to download it
            if (!File.Exists(Path.Combine(LoadManager.DecksDirectoryPath, deckInfo.GetCategory().ToString(), deckInfo.GetName() + ".json"))) 
            {
                isDeckDownloaded[0] = true;
                yield break; 
            }

            JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, deckInfo.GetCategory().ToString(), deckInfo.GetName() + ".json")));
            List<bool> isCardDownloaded = new() { false };

            for (int i = 0; i < jsonCards.cards.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + jsonCards.cards.Count + ") " + jsonCards.cards[i].anime;

                // Check if visual is downloaded and download if not
                if (!IsCardVisualDownloaded(jsonCards.cards[i].visual))
                {
                    yield return StartCoroutine(DownloadCardVisual(jsonCards.cards[i].visual, isCardDownloaded));

                    // If download fail, stop the download of the rest of the deck
                    if (!isCardDownloaded[0])
                    {
                        yield return ExitOnDownloadFail("Failed to download visual " + jsonCards.cards[i].visual);
                        isDeckDownloaded[0] = false;
                        yield break;
                    }
                }

                // Check if audio is downloaded and download if not
                if (!IsCardAudioDownloaded(jsonCards.cards[i].audio))
                {
                    yield return StartCoroutine(DownloadCardAudio(jsonCards.cards[i].audio, isCardDownloaded));

                    // If download fail, stop the download of the rest of the deck
                    if (!isCardDownloaded[0])
                    {
                        yield return ExitOnDownloadFail("Failed to download audio " + jsonCards.cards[i].audio);
                        isDeckDownloaded[0] = false;
                        yield break;
                    }
                }
            }
            isDeckDownloaded[0] = true;
        }

        /// <summary>
        /// Download a card visual
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="isDownloaded"></param>
        /// <returns></returns>
        private IEnumerator DownloadCardVisual(string visual, List<bool> isDownloaded)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "visual/" + visual);
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(LoadManager.VisualsDirectoryPath, visual));
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                isDownloaded[0] = true;
            }
            else
            {
                if (IsCardVisualDownloaded(visual))
                {
                    File.Delete(Path.Combine(LoadManager.VisualsDirectoryPath, visual));
                }
                isDownloaded[0] = false;
            }
        }

        /// <summary>
        /// Download a card audio
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="isDownloaded"></param>
        /// <returns></returns>
        private IEnumerator DownloadCardAudio(string audio, List<bool> isDownloaded)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "sound/" + audio);
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(LoadManager.AudioDirectoryPath, audio));
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                isDownloaded[0] = true;
            }
            else
            {
                if (IsCardAudioDownloaded(audio))
                {
                    File.Delete(Path.Combine(LoadManager.AudioDirectoryPath, audio));
                }
                isDownloaded[0] = false;
            }
        }
        #endregion Download Deck Content

        #region Check if downloaded
        /// <summary>
        /// Check if a deck is downloaded
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        private bool IsDeckDownloaded(DeckInfo deck)
        {
            JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, deck.GetCategory().ToString(), deck.GetName() + ".json")));

            foreach (JsonCard jsonCard in jsonCards.cards)
            {
                if (!IsCardDownloaded(jsonCard))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if a deck is downloaded
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        private bool IsDeckDownloaded(DownloadDeck deck)
        {
            JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, deck.category, deck.name + ".json")));

            foreach (JsonCard jsonCard in jsonCards.cards)
            {
                if (!IsCardDownloaded(jsonCard))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if the card is already downloaded in the system
        /// </summary>
        /// <param name="downloadCard"></param>
        /// <returns></returns>
        private bool IsCardDownloaded(JsonCard jsonCard)
        {
            return IsCardVisualDownloaded(jsonCard.visual) && IsCardAudioDownloaded(jsonCard.audio);
        }

        /// <summary>
        /// Check if the visual file is already downloaded in the system
        /// </summary>
        /// <param name="downloadCardVisual"></param>
        /// <returns></returns>
        private bool IsCardVisualDownloaded(string downloadCardVisual)
        {
            if (!Array.Exists(visualFiles, visualFile => visualFile == Path.Combine(LoadManager.VisualsDirectoryPath, downloadCardVisual)))
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
        private bool IsCardAudioDownloaded(string downloadCardAudio)
        {
            if (!Array.Exists(audioFiles, audioFile => audioFile == Path.Combine(LoadManager.AudioDirectoryPath, downloadCardAudio)))
            {
                //Debug.Log("D_ Audio " + downloadCardAudio + " does not exist")
                return false;
            }
            return true;
        }
        #endregion Check if downloaded

        private IEnumerator ExitOnDownloadFail(string error)
        {
            downloadingDeckTextMesh1.text = error;

            yield return new WaitForSeconds(2);

            waitingPanel.SetActive(false);
        }

        private void OnValidate()
        {
            togglesParent.spacing = toggleSpacing;

            foreach(Toggle toggle in toggles)
            {
                toggle.transform.localScale = toggleScale;
            }
        }
    }
}