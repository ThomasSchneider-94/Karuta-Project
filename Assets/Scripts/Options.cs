using Karuta;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Karuta
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
        [SerializeField] private Image categoryIcon;
        [SerializeField] private Image categoryButtonBackground;
        [SerializeField] private Sprite karutoIcon;
        [SerializeField] private Sprite karutaIcon;

        // Start is called before the first frame update
        void Start()
        {
            gameManager = GameManager.Instance;
            InitializeOptions();
        }

        // Initialize the visuals
        #region Options Toggles
        private void InitializeOptions()
        {
            autoPlayToggle.SetIsOnWithoutNotify(gameManager.GetAutoPlay());
            playPauseToggle.SetIsOnWithoutNotify(gameManager.GetPlayPause());
            hideAnswerToggle.SetIsOnWithoutNotify(gameManager.GetHideAnswer());
            allowMirrorMatchToggle.SetIsOnWithoutNotify(gameManager.GetMirrorMatches());
            allowDifferentCategoriesToggle.SetIsOnWithoutNotify(gameManager.GetDifferentCategory());
            UpdateCategoryIcon();
        }

        public void UpdateAutoPlay()
        {
            gameManager.SetAutoPlay(autoPlayToggle.isOn);
        }

        public void UpdatePlayPause()
        {
            gameManager.SetPlayPause(playPauseToggle.isOn);
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
            categoryButtonBackground.gameObject.SetActive(!gameManager.GetDifferentCategory());
            switch (gameManager.GetCurrentCategory())
            {
                case DeckInfo.DeckCategory.Karuta:
                    categoryIcon.sprite = karutaIcon;
                    break;
                case DeckInfo.DeckCategory.Karuto:
                    categoryIcon.sprite = karutoIcon;
                    break;
                default:
                    break;
            }
        }
        #endregion Decks Category
    }
}
