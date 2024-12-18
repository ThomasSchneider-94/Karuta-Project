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
        [SerializeField] private Button deleteDeckButton;
        [SerializeField] private SelectionButton enableDeleteButton;

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
         *      - Supress deck
         */

        private bool connexionError = false;
        private bool downloadFail = false;
        private bool deletingModeOn = false;

        private void OnEnable()
        {
            loadManager = LoadManager.Instance;

            // LoadManager Events
            DecksManager.Instance.UpdateDeckListEvent.AddListener(UpdateToggles);
            OptionsManager.Instance.UpdateCategoryEvent.AddListener(HideNonUsedToggles);

            waitingPanel.SetActive(false);
            downloadDeckButton.interactable = false;
            deleteDeckButton.interactable = false;
            deletingModeOn = false;

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

                List<string> deckContents = new();
                                
                for (int i = 0; i < deckNames.Count; i++)
                {
                    downloadingDeckTextMesh1.text = "(" + (i + 1) + "/" + deckNames.Count + ")\n" + deckNames[i];

                    yield return StartCoroutine(DownloadDeckInformation(deckNames[i], deckContents));

                    // Break on connexion error
                    if (connexionError) { yield break; }

                    if (deckContents[^1] != null)
                    {
                        DownloadDeck downloadDeck = JsonUtility.FromJson<DownloadDeck>(deckContents[^1]);

                        yield return StartCoroutine(DownloadDeckCover(downloadDeck.cover));

                        // Break on connexion error
                        if (connexionError) { yield break; }

                        JsonDeckInfo deckInfo = SaveDeck(downloadDeck);

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

                List<string> deckContents = new();

                for (int i = 0; i < deckNames.Count; i++)
                {
                    downloadingDeckTextMesh1.text = "(" + (i + 1) + "/" + deckNames.Count + ")\n" + deckNames[i];

                    deckContents.Add(File.ReadAllText(deckNames[i]));

                    DownloadDeck downloadDeck = JsonUtility.FromJson<DownloadDeck>(deckContents[^1]);
                    JsonDeckInfo deckInfo = SaveDeck(downloadDeck);

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
                yield return StartCoroutine(ExitOnConnexionError("Connexion Error"));
            }
            else
            {
                Debug.LogWarning("Downloading deck list failed");
                yield return StartCoroutine(ExitOnConnexionError("Downloading deck list failed"));
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
                yield return StartCoroutine(ExitOnConnexionError("Connexion Error"));
            }
            else
            {
                Debug.LogWarning("Downloading deck " + deckName + " content failed");
                deckContents.Add(null);
            }
        }





        private IEnumerator DownloadDeckCover(string cover)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "cover/" + cover);
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(LoadManager.CoversDirectoryPath, cover));
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success && File.Exists(Path.Combine(LoadManager.CoversDirectoryPath, cover)))
            {
                File.Delete(Path.Combine(LoadManager.CoversDirectoryPath, cover));
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                yield return ExitOnConnexionError("Connexion Error");
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning(cover + " file does not exist");
            }
        }





        /// <summary>
        ///  Save a deck  and its cards into the persistant files
        /// </summary>
        /// <param name="textDeck"></param>
        /// <returns></returns>
        private JsonDeckInfo SaveDeck(DownloadDeck downloadDeck)
        {
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

        /// <summary>
        /// Check if a deck is downloaded
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        private static bool IsDeckDownloaded(DownloadDeck deck)
        {
            foreach (JsonCard card in deck.cards)
            {
                if (!File.Exists(Path.Combine(LoadManager.VisualsDirectoryPath, card.visual)))
                {
                    return false;
                }
                if (!File.Exists(Path.Combine(LoadManager.AudioDirectoryPath, card.audio)))
                {
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

            toggle.onValueChanged.AddListener(delegate { CheckTogglesSelection(); });

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
                // If a deck is downloaded, make it appears when deletingModeOn is true
                if (deletingModeOn == deckList[i].IsDownloaded() && OptionsManager.Instance.IsCategoryActive(deckList[i].GetCategory()))
                {
                    toggles[i].gameObject.SetActive(true);
                }
                else
                {
                    toggles[i].gameObject.SetActive(false);
                    toggles[i].SetIsOnWithoutNotify(false);
                }
            }

            CheckTogglesSelection();
        }

        /// <summary>
        /// Ckeck if one or all active toggles are on
        /// </summary>
        private void CheckTogglesSelection()
        {
            bool one = false;
            bool all = true;

            foreach (LabeledToggle toggle in toggles)
            {
                if (toggle.gameObject.activeSelf)
                {
                    if (!one && toggle.isOn)
                    {
                        // If one is found to be checked
                        one = true;
                    }
                    else if (all && !toggle.isOn)
                    {
                        // If one is not checked
                        all = false;
                    }
                    else
                    {
                        // If one is found checked and one not checked
                        break;
                    }
                }
            }

            selectAllToggle.SetIsOnWithoutNotify(all);
            downloadDeckButton.interactable = one;
            deleteDeckButton.interactable = one;
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
            deleteDeckButton.interactable = isOn;
        }
        #endregion Toggles Gestion

        public List<LabeledToggle> GetToggles()
        {
            return toggles;
        }

        #endregion Toggles


        public void SwitchToDeleteMode()
        {
            deletingModeOn = !deletingModeOn;

            if (deletingModeOn)
            {
                enableDeleteButton.SelectButton();
            }
            else
            {
                enableDeleteButton.DeselectButton();
            }
        }

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
            // Initialization
            waitingPanel.SetActive(true);
            connexionError = false;

            List<DeckInfo> decksToDownload = new();
            List<DeckInfo> decksToNothing = new();
            List<DeckInfo> deckList = DecksManager.Instance.GetDeckList();

            JsonDeckInfoList jsonDeckInfoList = new()
            {
                deckInfoList = new()
            };

            // Separate the deck between those to download and the others
            for (int i = 0; i < deckList.Count; i++)
            {
                if (toggles[i].isOn)
                {
                    decksToDownload.Add(deckList[i]);
                }
                else
                {
                    decksToNothing.Add(deckList[i]);
                }
            }

            // Download the selected decks
            for (int i = 0; i < decksToDownload.Count; i++)
            {
                downloadingDeckTextMesh1.text = "(" + (i + 1) + "/" + decksToDownload.Count + ") " + decksToDownload[i].GetName();
                downloadFail = false;

                // If connexion error, do not try to download the deck, but add the others to the list
                if (!connexionError)
                {
                    yield return DownloadDeckContent(decksToDownload[i]);
                }

                jsonDeckInfoList.deckInfoList.Add(new JsonDeckInfo()
                {
                    name = decksToDownload[i].GetName(),
                    category = decksToDownload[i].GetCategory(),
                    type = decksToDownload[i].GetDeckType(),
                    cover = decksToDownload[i].GetCoverName(),
                    isDownloaded = !downloadFail && !connexionError
                });
            }

            // Rewrite all the other deck in the decks file
            foreach(DeckInfo deck in decksToNothing)
            {
                jsonDeckInfoList.deckInfoList.Add(new JsonDeckInfo()
                {
                    name = deck.GetName(),
                    category = deck.GetCategory(),
                    type = deck.GetDeckType(),
                    cover = deck.GetCoverName(),
                    isDownloaded = deck.IsDownloaded(),
                });
            }

            File.WriteAllText(LoadManager.DecksFilePath, JsonUtility.ToJson(jsonDeckInfoList));

            DecksManager.Instance.UpdateDeckList();

            waitingPanel.SetActive(false);
        }

        /// <summary>
        /// Download the visuals and audio of a deck
        /// </summary>
        /// <param name="deckInfo"></param>
        /// <returns></returns>
        private IEnumerator DownloadDeckContent(DeckInfo deckInfo)
        {
            // If the deck file does not exist, do not try to download it
            if (!File.Exists(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategoryName(deckInfo.GetCategory()), deckInfo.GetName() + ".json"))) 
            {
                yield break; 
            }

            JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategoryName(deckInfo.GetCategory()), deckInfo.GetName() + ".json")));

            for (int i = 0; i < jsonCards.cards.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + jsonCards.cards.Count + ") " + jsonCards.cards[i].anime;

                yield return StartCoroutine(DownloadCardVisual(jsonCards.cards[i].visual));

                // If connexion error, do not try to download other deck content
                if (connexionError) { yield break; }

                yield return StartCoroutine(DownloadCardAudio(jsonCards.cards[i].audio));

                // If connexion error, do not try to download other deck content
                if (connexionError) { yield break; }
            }
        }

        /// <summary>
        /// Download a card visual
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        private IEnumerator DownloadCardVisual(string visual)
        {
            if (File.Exists(Path.Combine(LoadManager.VisualsDirectoryPath, visual)))
            {
                // Already Dowloaded
                yield break;
            }

            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "visual/" + visual);
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(LoadManager.VisualsDirectoryPath, visual));
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success && File.Exists(Path.Combine(LoadManager.VisualsDirectoryPath, visual)))
            {
                downloadFail = true;
                File.Delete(Path.Combine(LoadManager.VisualsDirectoryPath, visual));
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                downloadFail = true;
                yield return ExitOnConnexionError("Connexion Error");
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                downloadFail = true;
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
            if (File.Exists(Path.Combine(LoadManager.AudioDirectoryPath, audio)))
            {
                // Already Dowloaded
                yield break;
            }

            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "sound/" + audio);
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(LoadManager.AudioDirectoryPath, audio));
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success && File.Exists(Path.Combine(LoadManager.AudioDirectoryPath, audio)))
            {
                downloadFail = true;
                File.Delete(Path.Combine(LoadManager.AudioDirectoryPath, audio));
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                downloadFail = true;
                yield return ExitOnConnexionError("Connexion Error");
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                downloadFail = true;
                Debug.LogWarning(audio + " file does not exist");
            }
        }
        #endregion Download Deck Content

        #region Delete Deck Content
        /// <summary>
        /// Delete the content of selected decks
        /// </summary>
        public void DeleteSelected()
        {
            // Initialization
            waitingPanel.SetActive(true);
            downloadingDeckTextMesh1.text = "Searching for files";

            List<string> visualFiles = new(Directory.GetFiles(LoadManager.VisualsDirectoryPath));
            List<string> audioFiles = new(Directory.GetFiles(LoadManager.AudioDirectoryPath));

            // Remove the files used in the other decks
            RemoveUsedFiles(visualFiles, audioFiles);

            // Delete all the remainings files in the list
            DeleteFiles(visualFiles, audioFiles);

            // Save the new decks information
            RewriteDecksAfterDelete();

            DecksManager.Instance.UpdateDeckList();

            waitingPanel.SetActive(false);
        }

        /// <summary>
        /// Browse through the downloaded deck to remove the files used in other decks
        /// </summary>
        /// <param name="visualFiles"></param>
        /// <param name="audioFiles"></param>
        private void RemoveUsedFiles(List<string> visualFiles, List<string> audioFiles)
        {
            List<DeckInfo> deckList = DecksManager.Instance.GetDeckList();
            for (int i = 0; i < deckList.Count; i++)
            {
                // Remove all files from the list that appears in other decks
                if (deckList[i].IsDownloaded() && !toggles[i].isOn)
                {
                    JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(
                        File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath,
                            DecksManager.Instance.GetCategoryName(deckList[i].GetCategory()), deckList[i].GetName() + ".json")
                        )
                    );

                    foreach (JsonCard card in jsonCards.cards)
                    {
                        visualFiles.Remove(card.visual);
                        audioFiles.Remove(card.audio);
                    }
                }
            }
        }

        /// <summary>
        /// Delete the files from the list
        /// </summary>
        /// <param name="visualFiles"></param>
        /// <param name="audioFiles"></param>
        private void DeleteFiles(List<string> visualFiles, List<string> audioFiles)
        {
            int j = 0;
            foreach (string visualFile in visualFiles)
            {
                File.Delete(Path.Combine(LoadManager.VisualsDirectoryPath, visualFile));
                downloadingDeckTextMesh1.text = "(" + j + "/" + (visualFiles.Count + audioFiles.Count) + ") Deleting " + visualFile;
                j++;
            }
            foreach (string audioFile in audioFiles)
            {
                File.Delete(Path.Combine(LoadManager.AudioDirectoryPath, audioFile));
                downloadingDeckTextMesh1.text = "(" + j + "/" + (visualFiles.Count + audioFiles.Count) + ") Deleting " + audioFile;
                j++;
            }
        }

        /// <summary>
        /// Save the decks information
        /// </summary>
        private void RewriteDecksAfterDelete()
        {
            List<DeckInfo> deckList = DecksManager.Instance.GetDeckList();
            JsonDeckInfoList jsonDeckInfoList = new()
            {
                deckInfoList = new()
            };
            for (int i = 0; i < deckList.Count; i++)
            {
                jsonDeckInfoList.deckInfoList.Add(new JsonDeckInfo()
                {
                    name = deckList[i].GetName(),
                    category = deckList[i].GetCategory(),
                    type = deckList[i].GetDeckType(),
                    cover = deckList[i].GetCoverName(),
                    isDownloaded = deckList[i].IsDownloaded() && !toggles[i].isOn,
                });
            }



            File.WriteAllText(LoadManager.DecksFilePath, JsonUtility.ToJson(jsonDeckInfoList));
        }
        #endregion Delete Deck Content

        private IEnumerator ExitOnConnexionError(string error)
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