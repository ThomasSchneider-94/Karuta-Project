using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using Karuta.UIComponent;
using Karuta.Objects;

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
         *      - Download Categories
         */

        // Files 
        string[] visualFiles;
        string[] audioFiles;

        private bool connexionError = false;

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
            connexionError = false;

            JsonDeckInfoList jsonDeckInfoList = new()
            {
                deckInfoList = new()
            };

            // Download the list of decks name
            List<string> deckNames = new();
            if (!debugOn)
            {
                yield return StartCoroutine(DownloadDeckList(deckNames));

                // If fail to get deck list, break
                if (connexionError) { yield break; }

                // List Init 
                visualFiles = Directory.GetFiles(LoadManager.VisualsDirectoryPath);
                audioFiles = Directory.GetFiles(LoadManager.AudioDirectoryPath);
                List<string> deckContents = new();
                                
                for (int i = 0; i < deckNames.Count; i++)
                {
                    downloadingDeckTextMesh1.text = "(" + (i + 1) + "/" + deckNames.Count + ")\n" + deckNames[i];

                    yield return StartCoroutine(DownloadDeckInformation(deckNames[i], deckContents));

                    // Break on connexion error
                    if (connexionError) { yield break; }

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
            }
            else
            {
                deckNames = new(Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Debug")));

                // List Init 
                visualFiles = Directory.GetFiles(LoadManager.VisualsDirectoryPath);
                audioFiles = Directory.GetFiles(LoadManager.AudioDirectoryPath);
                List<string> deckContents = new();

                for (int i = 0; i < deckNames.Count; i++)
                {
                    downloadingDeckTextMesh1.text = "(" + (i + 1) + "/" + deckNames.Count + ")\n" + deckNames[i];

                    deckContents.Add(File.ReadAllText(deckNames[i]));

                    JsonDeckInfo deckInfo = SaveDeck(deckContents[^1]);

                    jsonDeckInfoList.deckInfoList.Add(deckInfo);
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
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning("Connexion Error");
                yield return StartCoroutine(ExitOnDownloadFail("Connexion Error"));
            }
            else
            {
                Debug.LogWarning("Downloading deck list failed");
                yield return StartCoroutine(ExitOnDownloadFail("Downloading deck list failed"));
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
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                // If there is a connexion error, it is useless to try to download other decks
                Debug.LogWarning("Connexion Error");
                yield return StartCoroutine(ExitOnDownloadFail("Connexion Error"));
            }
            else
            {
                Debug.LogWarning("Downloading deck " + deckName + " content failed");
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
                category = DecksManager.Instance.GetCategories().IndexOf(downloadDeck.category),
                type = DecksManager.Instance.GetTypes().IndexOf(downloadDeck.type),
                cover = downloadDeck.cover,
                isDownloaded = IsDeckDownloaded(downloadDeck),
            };
        }
        
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
            if (!DecksManager.Instance.GetCategories().Contains(downloadDeck.category)) // Category
            {
                Debug.Log("D_ " + string.Format("Deck {0} category do not match Enum: {1}", downloadDeck.name, downloadDeck.category));
                return false; 
            }
            if (!DecksManager.Instance.GetTypes().Contains(downloadDeck.type)) // Type
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
            togglesParent.transform.localScale = toggleScale;

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

            // Set Toggle Values
            toggle.SetLabel(deckName);
            toggle.SetIsOnWithoutNotify(false);
            toggle.transform.localScale = Vector2.one;

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
                connexionError = false;

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

                        yield return DownloadDeckContent(deckList[i]);

                        // If connexion error, do not try to download other deck, but check if others has been downloaded
                        if (connexionError) { break; }

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
                        category = deckList[i].GetCategory(),
                        type = deckList[i].GetDeckType(),
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
        /// <returns></returns>
        private IEnumerator DownloadDeckContent(DeckInfo deckInfo)
        {
            // If the deck file does not exist, do not try to download it
            if (!File.Exists(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategory(deckInfo.GetCategory()), deckInfo.GetName() + ".json"))) 
            {
                yield break; 
            }

            JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategory(deckInfo.GetCategory()), deckInfo.GetName() + ".json")));

            for (int i = 0; i < jsonCards.cards.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + jsonCards.cards.Count + ") " + jsonCards.cards[i].anime;

                // Check if visual is downloaded and download if not
                if (!IsCardVisualDownloaded(jsonCards.cards[i].visual))
                {
                    yield return StartCoroutine(DownloadCardVisual(jsonCards.cards[i].visual));

                    // If connexion error, do not try to download other deck content
                    if (connexionError) { yield break; }
                }

                // Check if audio is downloaded and download if not
                if (!IsCardAudioDownloaded(jsonCards.cards[i].audio))
                {
                    yield return StartCoroutine(DownloadCardAudio(jsonCards.cards[i].audio));

                    // If connexion error, do not try to download other deck content
                    if (connexionError) { yield break; }
                }
            }
        }

        /// <summary>
        /// Download a card visual
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        private IEnumerator DownloadCardVisual(string visual)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "visual/" + visual);
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(LoadManager.VisualsDirectoryPath, visual));
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success && IsCardVisualDownloaded(visual))
            {
                File.Delete(Path.Combine(LoadManager.VisualsDirectoryPath, visual));
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                yield return ExitOnDownloadFail("Connexion Error");
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning(visual + " file does not exist");
            }
        }

        /// <summary>
        /// Download a card audio
        /// </summary>
        /// <param name="audio"></param>
        /// <returns></returns>
        private IEnumerator DownloadCardAudio(string audio)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "sound/" + audio);
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(LoadManager.AudioDirectoryPath, audio));
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success && IsCardAudioDownloaded(audio))
            {
                File.Delete(Path.Combine(LoadManager.AudioDirectoryPath, audio));
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                yield return ExitOnDownloadFail("Connexion Error");
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning(audio + " file does not exist");
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
            JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategory(deck.GetCategory()), deck.GetName() + ".json")));

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
            foreach (JsonCard jsonCard in deck.cards)
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
                return false;
            }
            return true;
        }
        #endregion Check if downloaded

        private IEnumerator ExitOnDownloadFail(string error)
        {
            connexionError = true;
            downloadingDeckTextMesh1.text = error;

            yield return new WaitForSeconds(2);

            waitingPanel.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            togglesParent.spacing = toggleSpacing;
            togglesParent.transform.localScale = toggleScale;
        }
#endif
    }
}