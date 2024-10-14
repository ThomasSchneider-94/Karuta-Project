using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Karuta.ScriptableObjects;
using static Karuta.ScriptableObjects.JsonObjects;
using UnityEngine.Events;
using System.Linq;
using Unity.VisualScripting;
using System.Threading;

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

        [Header("Decks")]
        [Range(0, 4)]
        [SerializeField] private int chosenDecksNumber = 2;
        [SerializeField] private List<String> chosenDecksName;
        private readonly List<DeckInfo> chosenDecks = new ();

        [Header("Default Sprite")]
        [SerializeField] private Sprite defaultSprite;

        /* The deck list is sorted like this :
         *  - First sorted on the deck category,
         *  - Secondly sorted on the deck type,
         *  - Then the decks are sorted by alphabetical order.
         *  
         *   The list decksNumbers is the count deck for each category / type couple.
         */
        private readonly List<DeckInfo> deckInfoList = new ();
        private readonly List<int> decksCount = new((int)DeckInfo.DeckCategory.CATEGORY_NB * (int)DeckInfo.DeckType.TYPE_NB);

        public UnityEvent UpdateCategoryEvent { get; } = new UnityEvent();
        public UnityEvent UpdateDeckListEvent { get; } = new UnityEvent();

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

        private void Start()
        {
            UpdateDeckList();
        }

        #region Deck List
        /// <summary>
        /// Update list of decks information
        /// </summary>
        public void UpdateDeckList()
        {
            deckInfoList.Clear();
            decksCount.Clear();

            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB * (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                decksCount.Add(0);
            }

            LoadDecksInformations();

            Debug.Log("Invoke Load Event");
            UpdateDeckListEvent.Invoke();
        }

        /// <summary>
        /// Load the list of decks information
        /// </summary>
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
                    decksCount[(int)deckInfo.GetCategory() * (int)DeckInfo.DeckType.TYPE_NB + (int)deckInfo.GetDeckType()]++;
                }
            }
            DecksInformationsQuickSort(0, deckInfoList.Count - 1);

            Debug.Log(Dump());
        }

        private void DecksInformationsQuickSort(int start, int end)
        {
            if (start >= end || start < 0) { return; }

            int pivot = DecksInformationsPartition(start, end);

            DecksInformationsQuickSort(start, pivot - 1);
            DecksInformationsQuickSort(pivot + 1 , end);
        }

        private int DecksInformationsPartition(int start, int end)
        {
            DeckInfo pivot = deckInfoList[end];

            int j = start;
            for (int i = start; i < end; i++)
            {
                if (!deckInfoList[i].IsGreaterThan(pivot))
                {
                    (deckInfoList[i], deckInfoList[j]) = (deckInfoList[j], deckInfoList[i]);
                    j++;
                }
            }
            (deckInfoList[j], deckInfoList[end]) = (deckInfoList[end], deckInfoList[j]);

            return j;
        }

        private string Dump()
        {
            StringBuilder dump = new ();

            dump.Append("Counts : [");
            foreach (int count in decksCount)
            {
                dump.Append(count + ", ");
            }
            dump.Append("]\n");

            dump.Append("Decks : [\n");
            foreach (DeckInfo deckInfo in deckInfoList)
            {
                dump.Append("| " + deckInfo.Dump() + "\n");
            }
            dump.Append("]");

            return dump.ToString();
        }

        #region Getter
        public List<int> GetDecksCount()
        {
            return decksCount;
        }

        public int GetTypeIndex(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            int start = GetCategoryIndex(category);
            for (int i = 0; i < (int)type; i++)
            {
                start += decksCount[(int)category * (int)DeckInfo.DeckType.TYPE_NB + i];
            }
            return start;
        }

        public int GetCategoryIndex(DeckInfo.DeckCategory category)
        {
            int start = 0;
            for (int i = 0; i < (int)category * (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                start += decksCount[i];
            }
            return start;
        }

        public int GetCategoryCount(DeckInfo.DeckCategory category)
        {
            int totalCount = 0;
            for (int i = 0; i < (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                totalCount += GetTypeCount(category, (DeckInfo.DeckType)i);
            }
            return totalCount;
        }

        public int GetTypeCount(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            return decksCount[(int)category * (int)DeckInfo.DeckType.TYPE_NB + (int)type];
        }

        public List<DeckInfo> GetDeckList()
        {
            return deckInfoList;
        }

        public List<DeckInfo> GetCategoryDeckList(DeckInfo.DeckCategory category)
        {
            return deckInfoList.GetRange(GetCategoryIndex(category), GetCategoryCount(category));
        }
        public List<DeckInfo> GetTypeDeckList(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            return deckInfoList.GetRange(GetTypeIndex(category, type), GetTypeCount(category, type));
        }
        #endregion Getter

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
            UpdateCategoryEvent.Invoke();
        }
        public void SetCurrentCategory(DeckInfo.DeckCategory category)
        {
            this.currentCategory = category;
            UpdateCategoryEvent.Invoke();
        }
        public void NextCurrentCategory()
        {
            this.currentCategory = (DeckInfo.DeckCategory)(((int)currentCategory + 1) % (int)DeckInfo.DeckCategory.CATEGORY_NB);
            UpdateCategoryEvent.Invoke();
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
        public bool IsCategoryActive(DeckInfo.DeckCategory category)
        {
            return allowDifferentCategory || category == currentCategory;
        }
        #endregion Getter

        #endregion Options

        #region Chosen Decks
        public bool IsChosenDecksFull()
        {
            return chosenDecks.Count >= chosenDecksNumber;
        }

        public void AddChosenDeck(DeckInfo deck)
        {
            if (chosenDecks.Count < chosenDecksNumber)
            {
                chosenDecks.Add(deck);
                chosenDecksName.Add(deck.GetName());
            }
        }

        public void RemoveChosenDeck(DeckInfo deck)
        {
            chosenDecks.Remove(deck);
            chosenDecksName.Remove(deck.GetName());
        }

        public List<DeckInfo> GetChosenDecks()
        {
            return chosenDecks;
        }
        #endregion Chosen Decks

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