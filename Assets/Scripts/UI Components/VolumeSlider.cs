using Karuta;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

namespace Karuta.UIComponent
{
    public class VolumeSlider : MonoBehaviour
    {
        private SoundManager soundManager;

        [Header("Sound Icons")]
        [SerializeField] private Sprite lowVolume;
        [SerializeField] private Sprite mediumVolume;
        [SerializeField] private Sprite highVolume;
        [SerializeField] private Sprite noVolume;

        [Header("Intern Objects")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TextMeshProUGUI volumeValueText;
        [SerializeField] private Image volumeIcon;
        
        void Start()
        {
            soundManager = SoundManager.Instance;

            InitSlider();
        }

        private void InitSlider()
        {
            volumeSlider.SetValueWithoutNotify(soundManager.GetGeneralVolumeValue());
            volumeValueText.text = ((int)(soundManager.GetGeneralVolumeValue() * 100)).ToString();
            UpdateSoundIcon();
        }

        // Set the sound icon to its corresponding value
        private void UpdateSoundIcon()
        {
            if (soundManager.IsGeneralVolumeOn())
            {
                float generalVolume = soundManager.GetGeneralVolumeValue();

                if (generalVolume <= 0) { volumeIcon.sprite = noVolume; }
                else if (generalVolume <= 0.33f) { volumeIcon.sprite = lowVolume; }
                else if (generalVolume >= 0.66f) { volumeIcon.sprite = highVolume; }
                else { volumeIcon.sprite = mediumVolume; }
            }
            else
            {
                volumeIcon.sprite = noVolume;
            }
        }

        public void UpdateVolumeValue()
        {
            soundManager.SetGeneralVolumeOn(true);
            soundManager.SetVolume(volumeSlider.value);
            volumeValueText.text = ((int)(soundManager.GetGeneralVolumeValue() * 100)).ToString();
            UpdateSoundIcon();
        }

        public void SwitchVolumeIsOn()
        {
            soundManager.SwitchIsGeneralVolumeOn();
            UpdateSoundIcon();
        }
    }
}
