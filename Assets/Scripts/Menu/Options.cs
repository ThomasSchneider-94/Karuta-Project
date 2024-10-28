using UnityEngine;
using UnityEngine.UI;
using Karuta.ScriptableObjects;
using Karuta.UIComponent;

namespace Karuta.Menu
{
    public class Options : MonoBehaviour
    {
        private GameManager gameManager;

        [Header("Toggles")]
        [SerializeField] private Toggle autoPlayToggle;
        [SerializeField] private Toggle playPauseToggle;
        [SerializeField] private Toggle hideAnswerToggle;
        [SerializeField] private Toggle allowMirrorMatchToggle;
        [SerializeField] private Toggle allowDifferentCategoriesToggle;

        [Header("Deck Category")]
        [SerializeField] private MultiLayerButton[] categoryButtons;
        [SerializeField] private Sprite[] categorySprites;

        // Start is called before the first frame update
        void Start()
        {
            if (categorySprites.Length < (int)DeckInfo.DeckCategory.CATEGORY_NB) { Debug.LogError("Not enought Category Sprite"); }

            gameManager = GameManager.Instance;
            InitializeOptions();
        }

        // Initialize the visuals
        #region Options Toggles
        private void InitializeOptions()
        {
            autoPlayToggle.SetIsOnWithoutNotify(gameManager.GetAutoPlay());
            hideAnswerToggle.SetIsOnWithoutNotify(gameManager.GetHideAnswer());
            allowMirrorMatchToggle.SetIsOnWithoutNotify(gameManager.AreMirrorMatchesAllowded());
            allowDifferentCategoriesToggle.SetIsOnWithoutNotify(gameManager.AreDifferentCategoryAllowded());
            UpdateCategoryIcon();
        }

        public void UpdateAutoPlay()
        {
            gameManager.SetAutoPlay(autoPlayToggle.isOn);
        }

        public void UpdateHideAnswer()
        {
            gameManager.SetHideAnswer(hideAnswerToggle.isOn);
        }

        public void UpdateMirrorMatch()
        {
            gameManager.SetMirrorMatches(allowMirrorMatchToggle.isOn);
        }
        #endregion Options Toggles

        #region Decks Category
        public void UpdateAllowDifferentCategories()
        {
            gameManager.SetDifferentCategory(allowDifferentCategoriesToggle.isOn);
            UpdateCategoryIcon();
        }

        public void NextCategory()
        {
            gameManager.NextCurrentCategory();
            UpdateCategoryIcon();
        }

        private void UpdateCategoryIcon()
        {
            foreach (MultiLayerButton categoryButton in categoryButtons)
            {
                categoryButton.gameObject.SetActive(!gameManager.AreDifferentCategoryAllowded());
                categoryButton.SetIconSprite(categorySprites[(int)gameManager.GetCurrentCategory()]);
            }            
        }
        #endregion Decks Category
    }
}
