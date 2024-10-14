using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Karuta
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("General Volume")]
        [SerializeField] private AudioMixer mixer;
        [SerializeField] float generalVolume = 0.5f;
        [SerializeField] bool isGeneralOn = true;

        // Start is called before the first frame update
        private void OnEnable()
        {
            // Assurez-vous qu'il n'y a qu'une seule instance du SoundManager
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

        #region Volume
        private void SetMixerVolume()
        {
            if (isGeneralOn && generalVolume > 0)
            {
                mixer.SetFloat("MasterVolume", 20f * Mathf.Log10(generalVolume * 2));
            }
            else
            {
                mixer.SetFloat("MasterVolume", 20 * Mathf.Log10(0.0000000000000000001f));
            }
        }
        public void SetVolume(float volume)
        {
            generalVolume = volume;
            SetMixerVolume();            
        }
        public float GetGeneralVolumeValue()
        {
            return generalVolume;
        }
        public void SetGeneralVolumeOn(bool isOn)
        {
            isGeneralOn = isOn;
            SetMixerVolume();
        }
        public void SwitchIsGeneralVolumeOn()
        {
            isGeneralOn = !isGeneralOn;
            SetMixerVolume();
        }
        public bool IsGeneralVolumeOn()
        {
            return isGeneralOn;
        }
        #endregion Volume
    }
}