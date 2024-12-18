using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Karuta.Game
{
    public class ArduinoHandler : MonoBehaviour
    {
        [SerializeField] private MainGame game;

        [Header("Audio")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private float volumeMinValue;
        [SerializeField] private string audioMixerParameter;

        [Header("Arduino")]
        [SerializeField] private int baudRate;


        // Start is called before the first frame update
        private void Start()
        {
            game.ColorIndicationUpdate.AddListener(UpdateLedColors);
        }

        private void Update()
        {
            float value = 100f;
            game.MoveCard(Vector2.zero, new Vector2(value, 0));
            game.OnMoveRelease(Vector2.zero, new Vector2(value, 0));
        }

        private void UpdateLedColors(float notFoundColorA, float foundColorA)
        {

        }

        private void UpdateAudioMixerVolume(float volume)
        {
            if (volume < volumeMinValue)
            {
                volume = volumeMinValue;
            }
            audioMixer.SetFloat(audioMixerParameter, 20f * Mathf.Log10(volume));
        }
    }
}