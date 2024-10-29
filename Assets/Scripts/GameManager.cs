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
        /* TODO : 
         * - Add arrow buttons to UI
         * - Add the selection for the number of deck
         * - The download (with loading screen)
         * - THE GAME
         */


        public static GameManager Instance { get; private set; }

        [Header("Options")]
        [SerializeField] private bool autoPlay = true;
        [SerializeField] private bool hideAnswer = false;
        [SerializeField] private bool allowMirrorMatch = false;
        [SerializeField] private bool allowDifferentCategory = false;
        [SerializeField] private DeckInfo.DeckCategory currentCategory = DeckInfo.DeckCategory.KARUTA;

        [Header("Decks")]
        [Range(0, 4)]
        [SerializeField] private int selectedDecksMax = 2;
        [SerializeField] private List<String> chosenDecksName = new();
        private readonly List<int> selectedDecks = new ();

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

        // Directory path
        public static string decksDirectoryPath;
        public static string coversDirectoryPath = Path.Combine(Application.persistentDataPath, "Covers");
        public static string visualsDirectoryPath = Path.Combine(Application.persistentDataPath, "Visuals");
        public static string audioDirectoryPath = Path.Combine(Application.persistentDataPath, "Audio");
        public static string themesDirectoryPath = Path.Combine(Application.persistentDataPath, "Themes");

        // Events
        public UnityEvent InitializePathesEvent{ get; } = new UnityEvent();
        public UnityEvent UpdateCategoryEvent { get; } = new UnityEvent();
        public UnityEvent InitializeDeckListEvent { get; } = new UnityEvent();
        public UnityEvent UpdateDeckListEvent { get; } = new UnityEvent();
        public UnityEvent UpdateMirorMatchEvent { get; } = new UnityEvent();

        public void OnEnable()
        {
            // Be sure that there is only one instance of GameManager
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject); // Destroy if another GameManager exist
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializePathes();
            InitializeDeckList();
        }

        private static void InitializePathes()
        {
            decksDirectoryPath = Path.Combine(Application.persistentDataPath, "Decks");
            coversDirectoryPath = Path.Combine(Application.persistentDataPath, "Covers");
            visualsDirectoryPath = Path.Combine(Application.persistentDataPath, "Visuals");
            audioDirectoryPath = Path.Combine(Application.persistentDataPath, "Audio");
            themesDirectoryPath = Path.Combine(Application.persistentDataPath, "Themes");

            InitializePathesEvent.Invoke();
        }

        #region Load Deck List
        /// <summary>
        /// Initialize list of deck informations
        /// </summary>
        private void InitializeDeckList()
        {
            for (int i = 0; i < (int)DeckInfo.DeckCategory.CATEGORY_NB * (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                decksCount.Add(0);
            }

            LoadDecksInformations();

            Debug.Log("Invoke Initialize Deck list Event");
            InitializeDeckListEvent.Invoke();
        }

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

            Debug.Log("Invoke Update Deck list Event");
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
        #endregion Load Deck List

        #region Deck List Getter

        #region Count
        /// <summary>
        /// Get the number of deck of each category / type
        /// </summary>
        /// <returns></returns>
        public List<int> GetDecksCount()
        {
            return decksCount;
        }

        /// <summary>
        /// Get the number of deck of a category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int GetCategoryCount(DeckInfo.DeckCategory category)
        {
            int totalCount = 0;
            for (int i = 0; i < (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                totalCount += GetTypeCount(category, (DeckInfo.DeckType)i);
            }
            return totalCount;
        }

        /// <summary>
        /// Get the number of deck of a type
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetTypeCount(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            return decksCount[(int)category * (int)DeckInfo.DeckType.TYPE_NB + (int)type];
        }
        #endregion Count

        #region Index
        /// <summary>
        /// Get the index of the first deck of the category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int GetCategoryIndex(DeckInfo.DeckCategory category)
        {
            int start = 0;
            for (int i = 0; i < (int)category * (int)DeckInfo.DeckType.TYPE_NB; i++)
            {
                start += decksCount[i];
            }
            return start;
        }

        /// <summary>
        /// Get the index of the first deck of the type
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetTypeIndex(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            int start = GetCategoryIndex(category);
            for (int i = 0; i < (int)type; i++)
            {
                start += decksCount[(int)category * (int)DeckInfo.DeckType.TYPE_NB + i];
            }
            return start;
        }
        #endregion Index

        public List<DeckInfo> GetDeckList()
        {
            return deckInfoList;
        }

        public DeckInfo GetDeck(int i)
        {
            return deckInfoList[i];
        }

        public List<DeckInfo> GetCategoryDeckList(DeckInfo.DeckCategory category)
        {
            return deckInfoList.GetRange(GetCategoryIndex(category), GetCategoryCount(category));
        }
        public List<DeckInfo> GetTypeDeckList(DeckInfo.DeckCategory category, DeckInfo.DeckType type)
        {
            return deckInfoList.GetRange(GetTypeIndex(category, type), GetTypeCount(category, type));
        }
        #endregion Deck List Getter

        #region Options

        #region Setter
        public void SetAutoPlay(bool autoPlay)
        {
            this.autoPlay = autoPlay;
        }

        public void SetHideAnswer(bool hideAnswer)
        {
            this.hideAnswer = hideAnswer;
        }

        public void SetMirrorMatches(bool allowMirrorMatch)
        {
            this.allowMirrorMatch = allowMirrorMatch;
            UpdateMirorMatchEvent.Invoke();
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
            DeckInfo.DeckCategory currentCategoryTmp = this.currentCategory;

            this.currentCategory = (DeckInfo.DeckCategory)(((int)currentCategory + 1) % (int)DeckInfo.DeckCategory.CATEGORY_NB);
            while (this.currentCategory != currentCategoryTmp && GetCategoryCount(this.currentCategory) == 0)
            {
                this.currentCategory = (DeckInfo.DeckCategory)(((int)currentCategory + 1) % (int)DeckInfo.DeckCategory.CATEGORY_NB);
            }

            if (this.currentCategory != currentCategoryTmp)
            {
                UpdateCategoryEvent.Invoke();
            }

        }
        #endregion Setter

        #region Getter
        public bool GetAutoPlay()
        {
            return autoPlay;
        }

        public bool GetHideAnswer()
        {
            return hideAnswer;
        }

        public bool AreMirrorMatchesAllowded()
        {
            return allowMirrorMatch;
        }

        public bool AreDifferentCategoryAllowded()
        {
            return allowDifferentCategory;
        }

        public DeckInfo.DeckCategory GetCurrentCategory()
        {
            return currentCategory;
        }

        /// <summary>
        /// Get if the category is the currently active
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool IsCategoryActive(DeckInfo.DeckCategory category)
        {
            return allowDifferentCategory || category == currentCategory;
        }
        #endregion Getter

        #endregion Options

        #region Selected Decks
        public bool IsSelectedDecksFull()
        {
            return selectedDecks.Count >= selectedDecksMax;
        }

        public void AddSelectedDeck(int deckIndex)
        {
            if (selectedDecks.Count < selectedDecksMax)
            {
                selectedDecks.Add(deckIndex);
                chosenDecksName.Add(deckInfoList[deckIndex].GetName());
            }
        }

        public void RemoveSelectedDeck(int deckIndex)
        {
            while (selectedDecks.Contains(deckIndex))
            {
                selectedDecks.Remove(deckIndex);
                chosenDecksName.Remove(deckInfoList[deckIndex].GetName());
            }
        }

        public List<int> GetSelectedDecks()
        {
            return selectedDecks;
        }
        #endregion Selected Decks

        public Sprite LoadSprite(string folder, string fileName)
        {
            string filePath;
            if (File.Exists(Path.Combine(folder, fileName)))
            {
                filePath = Path.Combine(folder, fileName);
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