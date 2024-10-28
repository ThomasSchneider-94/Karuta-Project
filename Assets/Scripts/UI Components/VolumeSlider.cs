using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Karuta.UIComponent
{
    public class VolumeSlider : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private string sliderName;
        [SerializeField] private string audioMixerParameter;
        [SerializeField] private AudioMixer audioMixer;

        [Header("Icons")]
        [SerializeField] private Sprite highVolumeIcon;
        [SerializeField] private Sprite mediumVolumeIcon;
        [SerializeField] private Sprite lowVolumeIcon;
        [SerializeField] private Sprite noVolumeIcon;

        [Header("Intern Object")]
        [SerializeField] private Slider slider;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI nameTextMesh;
        [SerializeField] private TextMeshProUGUI valueTextMesh;
        [SerializeField] private Image volumeIcon;

        private bool volumeActive = true;

        // Start is called before the first frame update
        private void Start()
        {
            if (!audioMixer.GetFloat(audioMixerParameter, out float mixerValue))
            {
                Debug.LogError(audioMixerParameter + " is not a public parameter of the audioMixer " + audioMixer.ToString() + "; Value read: " + mixerValue);
            }

            nameTextMesh.text = sliderName;
            UpdateSliderValue();
        }

        public void SliderUpdate()
        {
            volumeActive = true;

            UpdateMixerValue();
        }

        private void UpdateMixerValue()
        {
            float sliderValue = slider.value;
            if (!volumeActive)
            {
                sliderValue = slider.minValue;
            }

            audioMixer.SetFloat(audioMixerParameter, 20f * Mathf.Log10(sliderValue));
            valueTextMesh.text = ((int)(100 * sliderValue)).ToString();

            UpdateSliderIcon();
        }

        private void UpdateSliderValue()
        {
            audioMixer.GetFloat(audioMixerParameter, out float mixerValue);

            mixerValue = Mathf.Pow(10, (mixerValue / 20f));

            slider.SetValueWithoutNotify(mixerValue);
            valueTextMesh.text = ((int)(100 * slider.value)).ToString();

            UpdateSliderIcon();
        }

        private void UpdateSliderIcon()
        {
            if (volumeActive && slider.value > slider.minValue)
            {
                if (slider.value < 0.33)
                {
                    volumeIcon.sprite = lowVolumeIcon;
                }
                else if (slider.value > 0.66)
                {
                    volumeIcon.sprite = highVolumeIcon;
                }
                else
                {
                    volumeIcon.sprite = mediumVolumeIcon;
                }
            }
            else
            {
                volumeIcon.sprite = noVolumeIcon;
            }
        }

        public void SwitchVolume()
        {
            volumeActive = !volumeActive;
            UpdateSliderIcon();
        }

        private void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            nameTextMesh.text = sliderName;

            bool test = audioMixer.GetFloat(audioMixerParameter, out float mixerValue);
            if (test)
            {
                Debug.Log("Parameter value : " + mixerValue);
            }
            UpdateSliderIcon();
        }
    }
}