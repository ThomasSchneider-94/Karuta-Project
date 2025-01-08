using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;
using Karuta.Objects;

namespace Karuta
{
    public class OptionsManager : MonoBehaviour
    {
        public static OptionsManager Instance { get; private set; }
        public enum OptionType
        {
            AUTOPLAY,
            HIDE_ANSWER,
            ALLOW_MIRROR_MATCH,
            ALLOW_DIFFERENT_CATEGORIES
        }

        [Header("Options")]
        [SerializeField] private bool autoPlay = true;
        [SerializeField] private bool hideAnswer = false;
        [SerializeField] private bool allowMirrorMatch = false;
        [SerializeField] private bool allowDifferentCategories = false;
        [SerializeField] private int currentCategory = 0;

        private readonly List<Sprite> categoriesIcons = new();

        public UnityEvent OptionsInitializedEvent { get; } = new();
        public UnityEvent UpdateCategoryEvent { get; } = new();
        public UnityEvent UpdateMirorMatchEvent { get; } = new();
        public UnityEvent UpdateHidenAnswerEvent { get; } = new();

        private bool initialized;

        private void Awake()
        {
            // Be sure that there is only one instance of OptionsManager
            if (Instance == null)
            {
                Instance = this;

                Initialize();
                DecksManager.Instance.UpdateCategoriesAndTypesEvent.AddListener(UpdateCategoryIcons);

                initialized = true;
            }
            else
            {
                Destroy(gameObject); // Destroy if another OptionsManager exist
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Initialize()
        {
            autoPlay = PlayerPrefs.GetInt("AUTOPLAY", 1) == 1;
            hideAnswer = PlayerPrefs.GetInt("HIDE_ANSWER", 0) == 1;
            allowMirrorMatch = PlayerPrefs.GetInt("ALLOW_MIRROR_MATCH", 0) == 1;
            allowDifferentCategories = PlayerPrefs.GetInt("ALLOW_DIFFERENT_CATEGORIES", 0) == 1;

            UpdateCategoryIcons();
        }

        private void UpdateCategoryIcons()
        {
            categoriesIcons.Clear();

            // Read the deck categories and deck types
            if (File.Exists(LoadManager.CategoriesFilePath))
            {
                CategoriesAndTypes categoriesAndTypes = JsonUtility.FromJson<CategoriesAndTypes>(File.ReadAllText(LoadManager.CategoriesFilePath));

                foreach (Category category in categoriesAndTypes.categories)
                {
                    categoriesIcons.Add(LoadManager.Instance.LoadCategoryVisualSprite(category.icon));
                }
            }

            if (currentCategory != Mathf.Min(currentCategory, categoriesIcons.Count - 1))
            {
                SetCurrentCategory(0);
            }
        }

        public bool IsInitialized()
        {
            return initialized;
        }

        // Start is called before the first frame update
        void Start()
        {
            OptionsInitializedEvent.Invoke();
        }

        #region Switch
        public void SwitchAutoPlay()
        {
            autoPlay = !autoPlay;
            PlayerPrefs.SetInt("AUTOPLAY", this.autoPlay ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SwitchHideAnswer()
        {
            hideAnswer = !hideAnswer;
            PlayerPrefs.SetInt("HIDE_ANSWER", this.hideAnswer ? 1 : 0);
            PlayerPrefs.Save();

            UpdateHidenAnswerEvent.Invoke();
        }

        public void SwitchAllowtMirrorMatch()
        {
            allowMirrorMatch = !allowMirrorMatch;
            PlayerPrefs.SetInt("ALLOW_MIRROR_MATCH", this.allowMirrorMatch ? 1 : 0);
            PlayerPrefs.Save();

            UpdateMirorMatchEvent.Invoke();
        }

        public void SwitchAllowDifferentCategories()
        {
            allowDifferentCategories = !allowDifferentCategories;
            PlayerPrefs.SetInt("ALLOW_DIFFERENT_CATEGORIES", this.allowDifferentCategories ? 1 : 0);
            PlayerPrefs.Save();

            UpdateCategoryEvent.Invoke();
        }
        #endregion Switch

        #region Current Category
        public void SetCurrentCategory(int category)
        {
            if (category >= categoriesIcons.Count)
            {
                Debug.LogWarning("Category " + category + " is greater than " + categoriesIcons.Count);
                return;
            }

            this.currentCategory = category;
            UpdateCategoryEvent.Invoke();
        }

        public void NextCurrentCategory()
        {
            int currentCategoryTmp = this.currentCategory;

            this.currentCategory = ((currentCategory + 1) % DecksManager.Instance.GetCategoriesCount());
            while (this.currentCategory != currentCategoryTmp && DecksManager.Instance.GetCategoryDecksCount(this.currentCategory) == 0)
            {
                this.currentCategory = ((currentCategory + 1) % DecksManager.Instance.GetCategoriesCount());
            }

            if (this.currentCategory != currentCategoryTmp)
            {
                UpdateCategoryEvent.Invoke();
            }
        }
        #endregion Current Category

        #region Getter
        public bool GetAutoPlay()
        {
            return autoPlay;
        }

        public bool AreAnswersHiden()
        {
            return hideAnswer;
        }

        public bool AreMirrorMatchesAllowded()
        {
            return allowMirrorMatch;
        }

        public bool AreDifferentCategoriesAllowded()
        {
            return allowDifferentCategories;
        }

        public int GetCurrentCategory()
        {
            return currentCategory;
        }

        /// <summary>
        /// Get if the category is the currently active
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool IsCategoryActive(int category)
        {
            return allowDifferentCategories || category == currentCategory;
        }
        #endregion Getter

        public Sprite GetCategoryIcon(int category)
        {
            if (categoriesIcons.Count == 0)
            {
                return LoadManager.Instance.GetDefaultSprite();
            }
            return categoriesIcons[category];
        }

        public Sprite GetCurrentCategoryIcon()
        {
            if (categoriesIcons.Count == 0)
            {
                return LoadManager.Instance.GetDefaultSprite();
            }
            return categoriesIcons[currentCategory];
        }
    }
}