using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Karuta
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Download Manager")]
        [SerializeField] private DownloadManager downloadManager;

        [Header("Options")]
        [SerializeField] private bool autoPlay = true;
        [SerializeField] private bool playPause = true;
        [SerializeField] private bool hideAnswer = false;
        [SerializeField] private bool allowMirrorMatch = false;
        [SerializeField] private bool allowDifferentCategory = false;
        [SerializeField] private DeckInfo.DeckCategory currentCategory = DeckInfo.DeckCategory.Karuta;

        [Header("Default Sprite")]
        [SerializeField] private Sprite defaultSprite;

        private readonly List<DeckInfo> deckInfoList = new ();

        public void OnEnable()
        {
            // Assurez-vous qu'il n'y a qu'une seule instance du GameManager
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject); // Détruisez les doublons
            }
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            LoadDecksInformations();
        }

        #region Set / Get Options
        public void SetAutoPlay(bool autoPlay)
        {
            this.autoPlay = autoPlay;
        }
        public bool GetAutoPlay()
        {
            return autoPlay;
        }
        public void SetPlayPause(bool playPause)
        {
            this.playPause = playPause;
        }
        public bool GetPlayPause()
        {
            return playPause;
        }
        public void SetHideAnswer(bool hideAnswer)
        {
            this.hideAnswer = hideAnswer;
        }
        public bool GetHideAnswer()
        {
            return hideAnswer;
        }
        public void SetMirrorMatches(bool allowMirrorMatch)
        {
            this.allowMirrorMatch = allowMirrorMatch;
        }
        public bool GetMirrorMatches()
        {
            return allowMirrorMatch;
        }
        public void SetDifferentCategory(bool differentCategory)
        {
            this.allowDifferentCategory = differentCategory;
            downloadManager.HideShowDeckDownloadToggles();
        }
        public bool GetDifferentCategory()
        {
            return allowDifferentCategory;
        }
        public void SetCurrentCategory(DeckInfo.DeckCategory category)
        {
            this.currentCategory = category;
            downloadManager.HideShowDeckDownloadToggles();
        }
        public void NextCurrentCategory()
        {
            this.currentCategory = (DeckInfo.DeckCategory)(((int)currentCategory + 1) % (int)DeckInfo.DeckCategory.CATEGORY_NB);
            downloadManager.HideShowDeckDownloadToggles();
        }
        public DeckInfo.DeckCategory GetCurrentCategory()
        {
            return currentCategory;
        }
        #endregion Set / Get  Options

        #region Deck List
        private void LoadDecksInformations()
        {
            // Définir le chemin du fichier
            string filePath = Path.Combine(Application.persistentDataPath, "DecksInfo.json");

            // Vérifier si le fichier existe
            if (File.Exists(filePath))
            {
                // Lire le contenu du fichier
                foreach (JsonDeckInfo jsonDeckInfo in JsonUtility.FromJson<JsonDeckInfoList>(File.ReadAllText(filePath)).deckInfoList)
                {
                    deckInfoList.Add(new DeckInfo(jsonDeckInfo));
                }
            }
            Debug.Log(Dump());
        }

        private string Dump()
        {
            StringBuilder dump = new ();
            dump.Append("Decks : [\n");
            foreach (DeckInfo deckInfo in deckInfoList)
            {
                dump.Append("| " + deckInfo.Dump() + "\n");
            }
            dump.Append("]");
            return dump.ToString();
        }

        public void RefreshDeckList()
        {
            deckInfoList.Clear();
            LoadDecksInformations();
        }

        public List<DeckInfo> GetDeckList()
        {
            return deckInfoList;
        }
        #endregion Deck List

        public Sprite LoadSprite(string folder, string fileName)
        {
            string filePath;
            if (File.Exists(Path.Combine(Application.persistentDataPath, folder, fileName + ".png"))) // .png
            {
                filePath = Path.Combine(Application.persistentDataPath, folder, fileName + ".png");
            }
            else if (File.Exists(Path.Combine(Application.persistentDataPath, folder, fileName + ".jpg"))) // .jpg
            {
                filePath = Path.Combine(Application.persistentDataPath, folder, fileName + ".jpg");
            }
            else
            {
                return defaultSprite;
            }

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            Texture2D texture = new(1, 1);
            texture.LoadImage(bytes);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = fileName;

            return sprite;
        }
    }
}