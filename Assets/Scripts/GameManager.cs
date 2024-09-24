using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Karuta.ScriptableObjects;
using static Karuta.ScriptableObjects.JsonObjects;
using UnityEngine.Events;

namespace Karuta
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Options")]
        [SerializeField] private bool autoPlay = true;
        [SerializeField] private bool playPause = true;
        [SerializeField] private bool hideAnswer = false;
        [SerializeField] private bool allowMirrorMatch = false;
        [SerializeField] private bool allowDifferentCategory = false;
        [SerializeField] private DeckInfo.DeckCategory currentCategory = DeckInfo.DeckCategory.KARUTA;

        [Header("Default Sprite")]
        [SerializeField] private Sprite defaultSprite;

        private readonly List<DeckInfo> deckInfoList = new ();
        public UnityEvent ChangeCategory { get; } = new UnityEvent();

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

            LoadDecksInformations();
        }

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
                    DeckInfo deckInfo = ScriptableObject.CreateInstance<DeckInfo>();
                    deckInfo.Init(jsonDeckInfo);
                    deckInfoList.Add(deckInfo);
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

        #region Options

        #region Setter
        public void SetAutoPlay(bool autoPlay)
        {
            this.autoPlay = autoPlay;
        }
        public void SetPlayPause(bool playPause)
        {
            this.playPause = playPause;
        }
        public void SetHideAnswer(bool hideAnswer)
        {
            this.hideAnswer = hideAnswer;
        }
        public void SetMirrorMatches(bool allowMirrorMatch)
        {
            this.allowMirrorMatch = allowMirrorMatch;
        }
        public void SetDifferentCategory(bool differentCategory)
        {
            this.allowDifferentCategory = differentCategory;
            ChangeCategory.Invoke();
        }
        public void SetCurrentCategory(DeckInfo.DeckCategory category)
        {
            this.currentCategory = category;
            ChangeCategory.Invoke();
        }
        public void NextCurrentCategory()
        {
            this.currentCategory = (DeckInfo.DeckCategory)(((int)currentCategory + 1) % (int)DeckInfo.DeckCategory.CATEGORY_NB);
            ChangeCategory.Invoke();
        }
        #endregion Setter

        #region Getter
        public bool GetAutoPlay()
        {
            return autoPlay;
        }
        public bool GetPlayPause()
        {
            return playPause;
        }
        public bool GetHideAnswer()
        {
            return hideAnswer;
        }
        public bool GetMirrorMatches()
        {
            return allowMirrorMatch;
        }
        public bool GetDifferentCategory()
        {
            return allowDifferentCategory;
        }
        public DeckInfo.DeckCategory GetCurrentCategory()
        {
            return currentCategory;
        }
        #endregion Getter

        #endregion Options
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