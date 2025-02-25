using Karuta.Objects;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Karuta.Menu
{
    [RequireComponent(typeof(DownloadPanelManager))]
    public class DeckContentManager : Downloader
    {
        public static string VisualEndPoint { get; } = "visual/";
        public static string AudioEndPoint { get; } = "sound/";

        private DownloadPanelManager downloadPanelManager;

        protected override void Awake()
        {
            base.Awake();

            downloadPanelManager = GetComponent<DownloadPanelManager>();
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

            List<DeckInfo> deckList = DecksManager.Instance.GetDeckList();

            JsonDeckInfoList jsonDeckInfoList = new()
            {
                deckInfoList = new()
            };

            List<int> indexesToDownload = downloadPanelManager.GetOnTogglesIndexes();
            int count = 0;
            foreach (int index in indexesToDownload)
            {
                downloadingDeckTextMesh1.text = "(" + (count + 1) + "/" + indexesToDownload.Count + ") " + deckList[index].DeckName;

                // If connexion error, do not try to download the Deck, but add the others to the list
                if (!connexionError)
                {
                    yield return DownloadDeckContent(deckList[index]);
                }

                jsonDeckInfoList.deckInfoList.Add(new JsonDeckInfo()
                {
                    name = deckList[index].DeckName,
                    category = deckList[index].Category,
                    type = deckList[index].DeckType,
                    cover = deckList[index].CoverName,
                    isDownloaded = !downloadFail && !connexionError,
                });
                count++;
            }

            foreach (int index in downloadPanelManager.GetOffTogglesIndexes())
            {
                jsonDeckInfoList.deckInfoList.Add(new JsonDeckInfo()
                {
                    name = deckList[index].DeckName,
                    category = deckList[index].Category,
                    type = deckList[index].DeckType,
                    cover = deckList[index].CoverName,
                    isDownloaded = deckList[index].IsDownloaded,
                });
            }

            File.WriteAllText(LoadManager.DecksFilePath, JsonUtility.ToJson(jsonDeckInfoList));

            DecksManager.Instance.UpdateDeckList(jsonDeckInfoList);

            waitingPanel.SetActive(false);
        }

        /// <summary>
        /// Download the visuals and audio of a Deck
        /// </summary>
        /// <param name="deckInfo"></param>
        /// <returns></returns>
        private IEnumerator DownloadDeckContent(DeckInfo deckInfo)
        {
            downloadFail = false;

            // If the Deck file does not exist, do not try to download it
            if (!File.Exists(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategoryName(deckInfo.Category), deckInfo.DeckName + ".json")))
            {
                yield break;
            }

            JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath, DecksManager.Instance.GetCategoryName(deckInfo.Category), deckInfo.DeckName + ".json")));

            for (int i = 0; i < jsonCards.cards.Count; i++)
            {
                downloadingDeckTextMesh2.text = "(" + (i + 1) + "/" + jsonCards.cards.Count + ") " + jsonCards.cards[i].anime;

                yield return CardVisualDownloader(jsonCards.cards[i].visual);

                // If connexion error, do not try to download other Deck content
                //if (connexionError) { yield break; }

                yield return CardAudioDownloader(jsonCards.cards[i].audio);

                // If connexion error, do not try to download other Deck content
                //if (connexionError) { yield break; }
            }
        }
        #endregion Download Deck Content

        #region File Downloader
        /// <summary>
        /// Download a card Visual
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        private IEnumerator CardVisualDownloader(string visual)
        {
            yield return FileDownloader(Path.Combine(LoadManager.VisualsDirectoryPath, visual), VisualEndPoint + visual);
        }

        /// <summary>
        /// Download a card audio
        /// </summary>
        /// <param name="audio"></param>
        /// <returns></returns>
        private IEnumerator CardAudioDownloader(string audio)
        {
            yield return FileDownloader(Path.Combine(LoadManager.AudioDirectoryPath, audio), AudioEndPoint + audio);
        }
        #endregion File Downloader

        #region Delete Deck Content
        /// <summary>
        /// Delete the content of selected decks
        /// </summary>
        public void DeleteSelected()
        {
            // Initialization
            List<string> visualFiles = new(Directory.GetFiles(LoadManager.VisualsDirectoryPath));
            List<string> audioFiles = new(Directory.GetFiles(LoadManager.AudioDirectoryPath));

            // Remove the files used in the other decks
            RemoveUsedFiles(visualFiles, audioFiles);

            // Delete all the remainings files in the list
            DeleteFiles(visualFiles, audioFiles);

            // Save the new decks information
            RewriteDecksAfterDelete();
        }

        /// <summary>
        /// Browse through the downloaded Deck to remove the files used in other decks
        /// </summary>
        /// <param name="visualFiles"></param>
        /// <param name="audioFiles"></param>
        private void RemoveUsedFiles(List<string> visualFiles, List<string> audioFiles)
        {
            List<DeckInfo> deckList = DecksManager.Instance.GetDeckList();

            foreach (int index in downloadPanelManager.GetOffTogglesIndexes())
            {
                if (deckList[index].IsDownloaded)
                {
                    JsonCards jsonCards = JsonUtility.FromJson<JsonCards>(
                        File.ReadAllText(Path.Combine(LoadManager.DecksDirectoryPath,
                            DecksManager.Instance.GetCategoryName(deckList[index].Category), deckList[index].DeckName + ".json")
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
                    name = deckList[i].DeckName,
                    category = deckList[i].Category,
                    type = deckList[i].DeckType,
                    cover = deckList[i].CoverName,
                    isDownloaded = deckList[i].IsDownloaded && downloadPanelManager.Toggles[i].isOn,
                });
            }

            File.WriteAllText(LoadManager.DecksFilePath, JsonUtility.ToJson(jsonDeckInfoList));

            DecksManager.Instance.UpdateDeckList(jsonDeckInfoList);
        }
        #endregion Delete Deck Content
    }
}