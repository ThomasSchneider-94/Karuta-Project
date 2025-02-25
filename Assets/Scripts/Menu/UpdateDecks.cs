using Karuta.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Karuta.Menu
{
    public class UpdateDecks : Downloader
    {
        [Header("Debug")]
        [SerializeField] private bool debugOn;

        public static string DeckNamesEndPoint { get; } = "deck/names";
        public static string DeckMetadataEndPoint { get; } = "deck/metadata/";
        public static string DeckCoverEndPoint { get; } = "deck/cover/";
        public static string CategoriesAndTypesEndPoint { get; } = "categories_and_types";
        public static string CategoryIconEndPoint { get; } = "category/icon/";

        #region Update
        public void StartUpdateAll()
        {
            StartCoroutine(UpdateAll());
        }

        /// <summary>
        /// Update the category and deck list
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateAll()
        {
            if (!debugOn)
            {
                waitingPanel.SetActive(true);
                connexionError = false;
                downloadFail = false;

                // Update Categories and Types
                CategoriesAndTypes categoriesAndTypes = new()
                {
                    categories = new(),
                    types = new()
                };
                List<string> decks = new();
                yield return DownloadCategoriesAndDecks(categoriesAndTypes, decks);

                // If there was a problem during the download, do not update
                if (downloadFail) { yield break; }

                // Download the categories visuals
                yield return DownloadCategoriesVisuals(categoriesAndTypes);

                // Update the categories and types
                File.WriteAllText(LoadManager.CategoriesFilePath, JsonUtility.ToJson(categoriesAndTypes));
                DecksManager.Instance.UpdateCategoriesAndTypes(categoriesAndTypes);

                // Save the Deck lists
                List<JsonDeck> jsonDecks = new();
                foreach (string jsonDeck in decks)
                {
                    jsonDecks.Add(JsonUtility.FromJson<JsonDeck>(jsonDeck));
                }
                JsonDeckInfoList jsonDeckInfoList = SaveDecks(jsonDecks);

                // Download the decks covers
                yield return DownloadDecksCovers(jsonDeckInfoList);

                // Update the Deck list
                File.WriteAllText(LoadManager.DecksFilePath, JsonUtility.ToJson(jsonDeckInfoList));
                DecksManager.Instance.UpdateDeckList(jsonDeckInfoList);

                waitingPanel.SetActive(false);
            }
            else
            {
                UpdateInformationOnDebug();
            }
        }

        /// <summary>
        /// Download the categories and decks data
        /// </summary>
        /// <param name="categoriesAndTypes"></param>
        /// <param name="deckContents"></param>
        /// <returns></returns>
        private IEnumerator DownloadCategoriesAndDecks(CategoriesAndTypes categoriesAndTypes, List<string> deckContents)
        {
            // Download the new categories and types
            yield return CategoriesAndTypesDownloader(categoriesAndTypes);

            // If connexion error encountered, stop the update
            if (connexionError) { yield break; }

            // Download the decks names
            List<string> deckNames = new();
            yield return DeckNamesDownloader(deckNames);

            // Download the information of each decks
            downloadingDeckTextMesh1.text = "Decks";
            for (int i = 0; i < deckNames.Count; i++)
            {
                // If connexion error encountered, stop the update
                if (connexionError) { yield break; }

                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + deckNames.Count + ")\n" + deckNames[i];

                yield return DeckDataDownloader(deckNames[i], deckContents);
            }
        }

        /// <summary>
        /// Download the visuals for all catagories
        /// </summary>
        /// <param name="categoriesAndTypes"></param>
        /// <returns></returns>
        private IEnumerator DownloadCategoriesVisuals(CategoriesAndTypes categoriesAndTypes)
        {
            downloadingDeckTextMesh1.text = "Categories";
            for (int i = 0; i < categoriesAndTypes.categories.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + categoriesAndTypes.categories.Count + ")\n" + categoriesAndTypes.categories[i].name;

                // If no connexion error
                if (!connexionError)
                {
                    yield return CategoryIconDownloader(categoriesAndTypes.categories[i].icon);
                }
            }
        }

        private IEnumerator DownloadDecksCovers(JsonDeckInfoList jsonDeckInfoList)
        {
            downloadingDeckTextMesh1.text = "Covers";
            for (int i = 0; i < jsonDeckInfoList.deckInfoList.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + jsonDeckInfoList.deckInfoList.Count + ")\n" + jsonDeckInfoList.deckInfoList[i].name;

                // If connexion, download the Deck Cover
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

            foreach (JsonDeck deck in jsonDecks)
            {
                JsonDeckInfo deckInfo = SaveDeck(deck);

                if (deckInfo != null)
                {
                    jsonDeckInfoList.deckInfoList.Add(deckInfo);
                }
            }

            return jsonDeckInfoList;
        }

        /// <summary>
        ///  Save a Deck  and its cards into the persistant files
        /// </summary>
        /// <param name="textDeck"></param>
        /// <returns></returns>
        private JsonDeckInfo SaveDeck(JsonDeck downloadDeck)
        {
            downloadDeck.category = downloadDeck.category.ToUpper();
            downloadDeck.type = downloadDeck.type.ToUpper();

            // Check Deck validity
            if (!IsDeckValid(downloadDeck)) { return null; }

            // Save Cards List
            if (!Directory.Exists(Path.Combine(LoadManager.DecksDirectoryPath, downloadDeck.category)))
            {
                // If the Category repertory for the Deck does not exist, create it
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
        /// Check if Deck is valid (do not check the number of jsonCards)
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
            if (!DecksManager.Instance.GetTypes().Contains(downloadDeck.type)) // CardType
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
        /// Check if a Deck is downloaded
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
        /// Update the Deck list with data from the debug directory
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
        #endregion Update

        #region String Downloaders
        /// <summary>
        /// Download the categories and types data and add them to categoriesAndTypes
        /// </summary>
        /// <param name="categoriesAndTypes"></param>
        /// <returns></returns>
        private IEnumerator CategoriesAndTypesDownloader(CategoriesAndTypes categoriesAndTypes)
        {
            yield return StringDownloader(CategoriesAndTypesEndPoint, result =>
            {
                CategoriesAndTypes categoriesAndTypesTMP = JsonUtility.FromJson<CategoriesAndTypes>(result);
                
                categoriesAndTypes.categories.AddRange(categoriesAndTypesTMP.categories);
                foreach(Category category in categoriesAndTypes.categories)
                {
                    category.name = category.name.ToUpper();
                }

                categoriesAndTypes.types.AddRange(categoriesAndTypesTMP.types);
                for (int i = 0; i < categoriesAndTypesTMP.types.Count; i++)
                {
                    categoriesAndTypes.types[i] = categoriesAndTypes.types[i].ToUpper();
                }
            });
        }

        /// <summary>
        /// Download the list of deck names
        /// </summary>
        /// <param name="deckNames"></param>
        /// <returns></returns>
        private IEnumerator DeckNamesDownloader(List<String> deckNames)
        {
            yield return StringDownloader(DeckNamesEndPoint, result =>
            {
                string[] names = result.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < names.Length; i++)
                {
                    deckNames.Add(names[i]);
                }
            });
        }

        /// <summary>
        /// Download the data of a deck
        /// </summary>
        /// <param name="deckName"></param>
        /// <param name="deckContents"></param>
        /// <returns></returns>
        private IEnumerator DeckDataDownloader(string deckName, List<string> deckContents)
        {
            yield return StringDownloader(DeckMetadataEndPoint + deckName, result =>
            {
                deckContents.Add(result);
            });
        }
        #endregion String Downloaders

        #region File Downloaders
        /// <summary>
        /// Download a Deck Cover
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>P
        private IEnumerator DeckCoverDownloader(string cover)
        {
            yield return FileDownloader(Path.Combine(LoadManager.CoversDirectoryPath, cover), DeckCoverEndPoint + cover);
        }

        /// <summary>
        /// Download a Category icon
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        private IEnumerator CategoryIconDownloader(string icon)
        {
            yield return FileDownloader(Path.Combine(LoadManager.CategoryVisualsDirectoryPath, icon), CategoryIconEndPoint + icon);
        }
        #endregion File Downloaders
    }
}