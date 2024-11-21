using Karuta.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

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
        [SerializeField] private DeckInfo.DeckCategory currentCategory = DeckInfo.DeckCategory.KARUTA;

        public UnityEvent OptionsInitializedEvent { get; } = new UnityEvent();
        public UnityEvent UpdateCategoryEvent { get; } = new UnityEvent();
        public UnityEvent UpdateMirorMatchEvent { get; } = new UnityEvent();
        public UnityEvent UpdateHidenAnswerEvent { get; } = new UnityEvent();


        private bool initialized;

        private void Awake()
        {
            // Be sure that there is only one instance of OptionsManager
            if (Instance == null)
            {
                Instance = this;

                Initialize();

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
        public void SetCurrentCategory(DeckInfo.DeckCategory category)
        {
            this.currentCategory = category;
            UpdateCategoryEvent.Invoke();
        }

        public void NextCurrentCategory()
        {
            DeckInfo.DeckCategory currentCategoryTmp = this.currentCategory;

            this.currentCategory = (DeckInfo.DeckCategory)(((int)currentCategory + 1) % (int)DeckInfo.DeckCategory.CATEGORY_NB);
            while (this.currentCategory != currentCategoryTmp && DecksManager.Instance.GetCategoryCount(this.currentCategory) == 0)
            {
                this.currentCategory = (DeckInfo.DeckCategory)(((int)currentCategory + 1) % (int)DeckInfo.DeckCategory.CATEGORY_NB);
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
            return allowDifferentCategories || category == currentCategory;
        }
        #endregion Getter
    }
}