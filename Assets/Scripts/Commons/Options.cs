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
        [SerializeField] private Sprite karutaIcon;
        [SerializeField] private Sprite karutoIcon;

        private void OnEnable()
        {
            optionsManager = OptionsManager.Instance;

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
        }

        public void NextCurrentCategory()
        {
            optionsManager.NextCurrentCategory();
            Sprite sprite = optionsManager.GetCurrentCategory() switch
            {
                DeckInfo.DeckCategory.KARUTA => karutaIcon,
                DeckInfo.DeckCategory.KARUTO => karutoIcon,
                _ => LoadManager.Instance.GetDefaultSprite(),
            };

            foreach (MultiLayerButton button in categoryButtons)
            {
                button.SetIconSprite(sprite);
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