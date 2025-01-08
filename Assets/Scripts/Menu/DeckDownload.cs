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
using UnityEngine.Events;
using System.Net;
using System.Linq;

namespace Karuta.Menu
{
    public class DeckDownload : MonoBehaviour
    {
        private LoadManager loadManager;

        [SerializeField] private int downloadTimeout;
        

        [Header("Download Toggles")]
        [SerializeField] private Toggle selectAllToggle;
        [SerializeField] private LabeledToggle togglePrefab;
        [SerializeField] private VerticalLayoutGroup togglesParent;
        [SerializeField] private float toggleSpacing;
        [SerializeField] private Vector2 toggleScale;
        private readonly List<LabeledToggle> toggles = new();

        [Header("Delete Mode")]
        [SerializeField] private Image panel;
        [SerializeField] private Color panelColorOnDelete;
        [SerializeField] private Button downloadDeckButton;
        [SerializeField] private Button deleteDeckButton;
        [SerializeField] private SelectionButton enableDeleteButton;

        [Header("Waiting Screen")]
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private TextMeshProUGUI downloadingDeckTextMesh1;
        [SerializeField] private TextMeshProUGUI downloadingDeckTextMesh2;

        [Header("Debug")]
        [SerializeField] private bool debugOn;

        private bool connexionError = false;
        private bool downloadFail = false;
        private bool deletingModeOn = false;

        public UnityEvent TogglesCreatedEvent { get; } = new();

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
                DecksManager.Instance.DeckManagerInitializedEvent.AddListener(Initialize);
            }
        }

        private void Initialize()
        {
            CreateToggles();
        }

        #region Update Information
        public void StartUpdateAllInformation()
        {
            StartCoroutine(UpdateAllInformation());
        }

        /// <summary>
        /// Download the deck list and update the local ones
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateAllInformation()
        {
            if (!debugOn)
            {
                waitingPanel.SetActive(true);
                connexionError = false;

                // Download all informations
                CategoriesAndTypes categoriesAndTypes = new()
                {
                    categories = new(),
                    types = new()
                };
                List<string> decks = new();
                yield return DownloadInformation(categoriesAndTypes, decks);

                // Download the categories visuals
                yield return DownloadCategoriesVisuals(categoriesAndTypes);

                // Update the categories and types
                File.WriteAllText(LoadManager.CategoriesFilePath, JsonUtility.ToJson(categoriesAndTypes));
                DecksManager.Instance.UpdateCategoriesAndTypes(categoriesAndTypes);

                // Save the deck lists
                List<JsonDeck> jsonDecks = new();
                foreach (string jsonDeck in decks)
                {
                    jsonDecks.Add(JsonUtility.FromJson<JsonDeck>(jsonDeck));
                }
                JsonDeckInfoList jsonDeckInfoList = SaveDecks(jsonDecks);

                // Download the decks covers
                yield return DownloadDecksCovers(jsonDeckInfoList);

                // Update the deck list
                File.WriteAllText(LoadManager.DecksFilePath, JsonUtility.ToJson(jsonDeckInfoList));
                DecksManager.Instance.UpdateDeckList(jsonDeckInfoList);

                waitingPanel.SetActive(false);
            }
            else
            {
                UpdateInformationOnDebug();
            }
        }

        private IEnumerator DownloadInformation(CategoriesAndTypes categoriesAndTypes, List<string> deckContents)
        {
            // Download the new categories and types
            yield return CategoriesAndTypesDownloader(categoriesAndTypes);

            // If connexion error encountered, stop the update
            if (connexionError) { yield break; }

            // Download the decks names
            List<string> deckNames = new();
            yield return DeckNamesDownloader(deckNames);

            // If connexion error encountered, stop the update
            if (connexionError) { yield break; }

            // Download the information of each decks
            downloadingDeckTextMesh1.text = "Decks";
            for (int i = 0; i < deckNames.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + deckNames.Count + ")\n" + deckNames[i];

                yield return DeckDataDownloader(deckNames[i], deckContents);

                // If connexion error encountered, stop the update
                if (connexionError) { yield break; }
            }
        }

        /*
        /// <summary>
        /// Download Categories, Types and Decks information
        /// </summary>
        /// <param name="categoriesAndTypes"></param>
        /// <param name="deckContents"></param>
        /// <returns></returns>
        private IEnumerator DownloadAllInformation(CategoriesAndTypes categoriesAndTypes, List<string> deckContents)
        {
            // Download the new categories and types
            yield return DownloadCategoriesAndTypes(categoriesAndTypes);

            // If connexion error encountered, stop the update
            if (connexionError) { yield break; }

            // Download the decks names
            List<string> deckNames = new();
            yield return DownloadDeckNames(deckNames);

            // If connexion error encountered, stop the update
            if (connexionError) { yield break; }

            // Download the information of each decks
            downloadingDeckTextMesh1.text = "Decks";
            for (int i = 0; i < deckNames.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + deckNames.Count + ")\n" + deckNames[i];

                yield return DownloadDeckInformation(deckNames[i], deckContents);

                // If connexion error encountered, stop the update
                if (connexionError) { yield break; }
            }
        }
        */
        /*
        /// <summary>
        /// Download the categories and types
        /// </summary>
        /// <param name="categoriesAndTypes"></param>
        /// <returns></returns>
        private IEnumerator DownloadCategoriesAndTypes(CategoriesAndTypes categoriesAndTypes)
        {
            // Initiate the category request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "get_categories_and_types");
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                CategoriesAndTypes categoriesAndTypesTMP = JsonUtility.FromJson<CategoriesAndTypes>(webRequest.downloadHandler.text);

                foreach (Category category in categoriesAndTypesTMP.categories)
                {
                    categoriesAndTypes.categories.Add(category);
                }
                foreach (string type in categoriesAndTypesTMP.types)
                {
                    categoriesAndTypes.types.Add(type);
                }
            }
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning("Connexion Error");
                yield return ExitOnConnexionError("Connexion Error");
            }
            else if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Downloading categories list failed");
            }
        }

        /// <summary>
        /// Dowload the name of all the decks
        /// </summary>
        /// <param name="deckNames"></param>
        /// <returns></returns>
        private IEnumerator DownloadDeckNames(List<String> deckNames)
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
                yield return ExitOnConnexionError("Connexion Error");
            }
            else
            {
                Debug.LogWarning("Downloading deck list failed");
                yield return ExitOnConnexionError("Downloading deck list failed");
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
                yield return ExitOnConnexionError("Connexion Error");
            }
            else
            {
                Debug.LogWarning("Downloading deck " + deckName + " information failed");
            }
        }*/

        private IEnumerator DownloadCategoriesVisuals(CategoriesAndTypes categoriesAndTypes)
        {
            downloadingDeckTextMesh1.text = "Categories";
            for (int i = 0; i < categoriesAndTypes.categories.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + categoriesAndTypes.categories.Count + ")\n" + categoriesAndTypes.categories[i].name;

                // If no connexion error
                if (!connexionError)
                {
                    //yield return CategoryVisualDownloader(categoriesAndTypes.categories[i].icon); TODO
                    yield return DownloadCategoryVisual(categoriesAndTypes.categories[i].icon);
                }
            }
        }

        private IEnumerator DownloadCategoryVisual(string visual)
        {
            Debug.Log(visual);

            // Initiate the category request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + "category/" + visual + "/icon");
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(LoadManager.CategoryVisualsDirectoryPath, visual));
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result != UnityWebRequest.Result.Success && File.Exists(Path.Combine(LoadManager.CategoryVisualsDirectoryPath, visual)))
            {
                Debug.Log("Delete " + visual);
                File.Delete(Path.Combine(LoadManager.CategoryVisualsDirectoryPath, visual));
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                downloadFail = true;
                yield return ExitOnConnexionError("Connexion Error");
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning(loadManager.GetServerIP() + "category/" + visual + "/icon" + " file does not exist");
            }
        }

        private IEnumerator DownloadDecksCovers(JsonDeckInfoList jsonDeckInfoList)
        {
            downloadingDeckTextMesh1.text = "Covers";
            for (int i = 0; i < jsonDeckInfoList.deckInfoList.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + jsonDeckInfoList.deckInfoList.Count + ")\n" + jsonDeckInfoList.deckInfoList[i].name;

                // If connexion, download the deck cover
                if (!connexionError)
                {
                    yield return DeckCoverDownloader(jsonDeckInfoList.deckInfoList[i].cover);
                }
            }
        }

        #region Save Decks
        private JsonDeckInfoList SaveDecks(List<JsonDeck> jsonDecks)
        {
            JsonDeckInfoList jsonDeckInfoList = new()
            {
                deckInfoList = new()
            };
            downloadingDeckTextMesh1.text = "Sauvegarde des decks";
            downloadingDeckTextMesh2.text = "";

            foreach(JsonDeck deck in jsonDecks)
            {
                JsonDeckInfo deckInfo = SaveDeck(deck);

                if (deckInfo != null)
                {
                    jsonDeckInfoList.deckInfoList.Add(deckInfo);
                }
            }

            Debug.Log(jsonDeckInfoList.deckInfoList.Count);
            return jsonDeckInfoList;
        }

        /// <summary>
        ///  Save a deck  and its cards into the persistant files
        /// </summary>
        /// <param name="textDeck"></param>
        /// <returns></returns>
        private JsonDeckInfo SaveDeck(JsonDeck downloadDeck)
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
        private static bool IsDeckValid(JsonDeck downloadDeck)
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
            foreach (JsonCard jsonCard in downloadDeck.cards)
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
        private static bool IsDeckDownloaded(JsonDeck deck)
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
        #endregion Save Decks

        /// <summary>
        /// Update the deck list with data from the debug directory
        /// </summary>
        private void UpdateInformationOnDebug()
        {
            // Update Categories and Types
            string categoriesAndTypes = File.ReadAllText(Path.Combine(Application.persistentDataPath, "Debug", "Categories", "Categories.json"));
            File.WriteAllText(LoadManager.CategoriesFilePath, categoriesAndTypes);

            DecksManager.Instance.UpdateCategoriesAndTypes(JsonUtility.FromJson<CategoriesAndTypes>(categoriesAndTypes));

            // Update Decks
            List<string> deckContents = new();
            JsonDeckInfoList jsonDeckInfoList = new()
            {
                deckInfoList = new()
            };

            List<string> deckNames = new(Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Debug", "Decks")));

            for (int i = 0; i < deckNames.Count; i++)
            {
                deckContents.Add(File.ReadAllText(deckNames[i]));

                JsonDeck deck = JsonUtility.FromJson<JsonDeck>(deckContents[^1]);

                JsonDeckInfo deckInfo = SaveDeck(deck);
                if (deckInfo != null) 
                {
                    jsonDeckInfoList.deckInfoList.Add(deckInfo);
                }
            }

            File.WriteAllText(LoadManager.DecksFilePath, JsonUtility.ToJson(jsonDeckInfoList));

            DecksManager.Instance.UpdateDeckList(jsonDeckInfoList);
        }
        #endregion Update Information

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

            TogglesCreatedEvent.Invoke();
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

            DecksManager.Instance.UpdateDeckList(jsonDeckInfoList);

            waitingPanel.SetActive(false);
        }

        /// <summary>
        /// Download the visuals and audio of a deck
        /// </summary>
        /// <param name="deckInfo"></param>
        /// <returns></returns>
        private IEnumerator DownloadDeckContent(DeckInfo deckInfo)
        {
            downloadFail = false;

            // If the deck file does not exist, do not try to download it
            if (!File.Exists(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategoryName(deckInfo.GetCategory()), deckInfo.GetName() + ".json"))) 
            {
                yield break; 
            }

            JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategoryName(deckInfo.GetCategory()), deckInfo.GetName() + ".json")));

            for (int i = 0; i < jsonCards.cards.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + jsonCards.cards.Count + ") " + jsonCards.cards[i].anime;

                yield return CardVisualDownloader(jsonCards.cards[i].visual);

                // If connexion error, do not try to download other deck content
                if (connexionError) { yield break; }

                yield return CardAudioDownloader(jsonCards.cards[i].audio);

                // If connexion error, do not try to download other deck content
                if (connexionError) { yield break; }
            }
        }
        #endregion Download Deck Content

        #region String Downloaders
        private IEnumerator CategoriesAndTypesDownloader(CategoriesAndTypes categoriesAndTypes)
        {
            yield return StringDownloader(LoadManager.CategoriesAndTypesEndPoint, result =>
            {
                CategoriesAndTypes categoriesAndTypesTMP = JsonUtility.FromJson<CategoriesAndTypes>(result);
                categoriesAndTypes.categories.AddRange(categoriesAndTypesTMP.categories);
                categoriesAndTypes.types.AddRange(categoriesAndTypesTMP.types);
            });
        }

        private IEnumerator DeckNamesDownloader(List<String> deckNames)
        {
            yield return StringDownloader(LoadManager.DeckNamesEndPoint, result =>
            {
                string[] names = result.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < names.Length; i++)
                {
                    deckNames.Add(names[i]);
                }
            });
        }

        private IEnumerator DeckDataDownloader(string deckName, List<string> deckContents)
        {
            yield return StringDownloader(LoadManager.DeckDataEndPoint + deckName, result =>
            {
                deckContents.Add(result);
            });
        }

        private IEnumerator StringDownloader(string endPoint, Action<string> onSuccess)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + endPoint);
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onSuccess.Invoke(webRequest.downloadHandler.text);
            }
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                // If there is a connexion error, it is useless to try to download other decks
                Debug.LogWarning("Connexion Error");
                yield return ExitOnConnexionError("Connexion Error");
            }
            else
            {
                Debug.LogWarning(loadManager.GetServerIP() + endPoint + " does not exist");
            }
        }
        #endregion String Downloaders

        #region File Downloaders
        /// <summary>
        /// Download a category icon
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        private IEnumerator CategoryVisualDownloader(string visual)
        {
            yield return FileDownloader(Path.Combine(LoadManager.CategoryVisualsDirectoryPath, visual), LoadManager.CategoriesAndTypesEndPoint + visual);
        }

        /// <summary>
        /// Download a deck cover
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>P
        private IEnumerator DeckCoverDownloader(string cover)
        {
            yield return FileDownloader(Path.Combine(LoadManager.CoversDirectoryPath, cover), LoadManager.CoversEndPoint + cover);
        }

        /// <summary>
        /// Download a card visual
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        private IEnumerator CardVisualDownloader(string visual)
        {
            yield return FileDownloader(Path.Combine(LoadManager.VisualsDirectoryPath, visual), LoadManager.VisualsEndPoint + visual);
        }

        /// <summary>
        /// Download a card audio
        /// </summary>
        /// <param name="audio"></param>
        /// <returns></returns>
        private IEnumerator CardAudioDownloader(string audio)
        {
            yield return FileDownloader(Path.Combine(LoadManager.AudioDirectoryPath, audio), LoadManager.AudioEndPoint + audio);
        }

        /// <summary>
        /// Download all type of files
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="file"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        private IEnumerator FileDownloader(string filePath, string endPoint)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(loadManager.GetServerIP() + endPoint);
            webRequest.downloadHandler = new DownloadHandlerFile(filePath);
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success && File.Exists(filePath))
            {
                downloadFail = true;
                File.Delete(filePath);
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                downloadFail = true;
                yield return ExitOnConnexionError("Connexion Error");
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                downloadFail = true;
                Debug.LogWarning(loadManager.GetServerIP() + endPoint + " file does not exist");
            }
        }
        #endregion File Downloaders


        #region Delete Deck Content
        public void SwitchToDeleteMode()
        {
            deletingModeOn = !deletingModeOn;
            Debug.Log("Delete Mode");

            if (deletingModeOn)
            {
                enableDeleteButton.SelectButton();
                panel.color = new(panelColorOnDelete.r, panelColorOnDelete.g, panelColorOnDelete.b, panel.color.a);
            }
            else
            {
                enableDeleteButton.DeselectButton();
                panel.color = new(1, 1, 1, panel.color.a);
            }

            downloadDeckButton.gameObject.SetActive(!deletingModeOn);
            deleteDeckButton.gameObject.SetActive(deletingModeOn);
            HideNonUsedToggles();
        }

        /// <summary>
        /// Disable Delete mode
        /// </summary>
        public void DisableDeleteModeOnClose()
        {
            if (deletingModeOn)
            {
                SwitchToDeleteMode();
            }
        }

        /// <summary>
        /// Delete the content of selected decks
        /// </summary>
        public void DeleteSelected()
        {
            // Initialization

            List<string> visualFiles = new(Directory.GetFiles(LoadManager.VisualsDirectoryPath));
            List<string> audioFiles = new(Directory.GetFiles(LoadManager.AudioDirectoryPath));

            Debug.Log(visualFiles.Count);
            Debug.Log(string.Join(", ", visualFiles));

            // Remove the files used in the other decks
            RemoveUsedFiles(visualFiles, audioFiles);

            Debug.Log(visualFiles.Count);
            Debug.Log(string.Join(", ", visualFiles));

            // Delete all the remainings files in the list
            DeleteFiles(visualFiles, audioFiles);

            // Save the new decks information
            RewriteDecksAfterDelete();
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
                        if (visualFiles.Contains(card.visual))
                        {
                            Debug.Log("Other deck contain :" + card.visual);
                        }
                        visualFiles.Remove(Path.Combine(LoadManager.VisualsDirectoryPath, card.visual));
                        audioFiles.Remove(Path.Combine(LoadManager.AudioDirectoryPath, card.audio));
                    }
                }
            }
        }

        /// <summary>
        /// Delete the files from the list
        /// </summary>
        /// <param name="visualFiles"></param>
        /// <param name="audioFiles"></param>
        private static void DeleteFiles(List<string> visualFiles, List<string> audioFiles)
        {
            foreach (string visualFile in visualFiles)
            {
                File.Delete(Path.Combine(LoadManager.VisualsDirectoryPath, visualFile));
            }
            foreach (string audioFile in audioFiles)
            {
                File.Delete(Path.Combine(LoadManager.AudioDirectoryPath, audioFile));
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

            DecksManager.Instance.UpdateDeckList(jsonDeckInfoList);
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