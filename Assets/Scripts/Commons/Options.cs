using Karuta.ScriptableObjects;
using Karuta.UIComponent;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using static Karuta.OptionsManager;

namespace Karuta.Commons
{
    [System.Serializable]
    public class OptionToggle
    {
        public Toggle toggle;
        public OptionsManager.OptionType optionType;
    }

    public class Options : MonoBehaviour
    {
        private OptionsManager optionsManager;

        [SerializeField] private List<OptionToggle> toggles;
        [SerializeField] private List<MultiLayerButton> categoryButtons;

        private void OnEnable()
        {
            optionsManager = OptionsManager.Instance;

            DecksManager.Instance.UpdateCategoriesEvent.AddListener(ShowCategoryButton);
            DecksManager.Instance.UpdateDeckListEvent.AddListener(ShowCategoryButton);
            optionsManager.UpdateCategoryEvent.AddListener(ShowCategoryButton);

            // Initialize
            if (optionsManager.IsInitialized())
            {
                Initialize();
            }
            else
            {
                optionsManager.OptionsInitializedEvent.AddListener(Initialize);
            }
        }

        private void Initialize()
        {
            foreach (OptionToggle toggle in toggles)
            {
                switch (toggle.optionType)
                {
                    case OptionType.AUTOPLAY:
                        toggle.toggle.SetIsOnWithoutNotify(optionsManager.GetAutoPlay());
                        break;
                    case OptionType.HIDE_ANSWER:
                        toggle.toggle.SetIsOnWithoutNotify(optionsManager.AreAnswersHiden());
                        break;
                    case OptionType.ALLOW_MIRROR_MATCH:
                        toggle.toggle.SetIsOnWithoutNotify(optionsManager.AreMirrorMatchesAllowded());
                        break;
                    case OptionType.ALLOW_DIFFERENT_CATEGORIES:
                        toggle.toggle.SetIsOnWithoutNotify(optionsManager.AreDifferentCategoriesAllowded());
                        break;
                    default:
                        break;
                }
            }

            ShowCategoryButton();
        }

        public void NextCurrentCategory()
        {
            optionsManager.NextCurrentCategory();
            Sprite sprite = optionsManager.GetCurrentCategoryIcon();

            foreach (MultiLayerButton button in categoryButtons)
            {
                button.SetIconSprite(sprite);
            }
        }

        private void ShowCategoryButton()
        {
            bool showButton;
            int categoryCount = DecksManager.Instance.GetCategoriesCount();
            if (optionsManager.AreDifferentCategoriesAllowded())
            {
                // If all categories accepted, do not show the button
                showButton = false;
            }
            else if (categoryCount <= 1)
            {
                showButton = false;
            }
            else
            {
                showButton = false;
                int activeCategoryCount = 0;
                for (int i = 0; i < categoryCount; i++)
                {
                    if (DecksManager.Instance.GetCategoryDecksCount(i) > 0)
                    {
                        activeCategoryCount++;
                        if (activeCategoryCount >= 2)
                        {
                            showButton = true;
                            break;
                        }
                    }
                }
            }

            foreach (MultiLayerButton button in categoryButtons)
            {
                button.gameObject.SetActive(showButton);
            }
        }

        #region Switch
        public void SwitchAutoPlay()
        {
            optionsManager.SwitchAutoPlay();
        }

        public void SwitchHideAnswer()
        {
            optionsManager.SwitchHideAnswer();
        }

        public void SwitchAllowtMirrorMatch()
        {
            optionsManager.SwitchAllowtMirrorMatch();
        }

        public void SwitchAllowDifferentCategories()
        {
            optionsManager.SwitchAllowDifferentCategories();
        }
        #endregion Switch
    }
}