using UnityEngine;
using System.IO.Ports;
using UnityEngine.Audio;
using System;

namespace Karuta.Game
{
    public class SerialHandler : MonoBehaviour
    {
        [SerializeField] private MainGame game;

        [Header("Serial")]
        [SerializeField] private string serialPort = "COM1";
        [SerializeField] private int baudRate = 115200;
        private readonly SerialPort serial = new();

        [Header("Audio")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private float volumeMinValue;
        [SerializeField] private string audioMixerParameter;

        [Header("Color")]
        [Range(0, 255)]
        [SerializeField] private int colorFactor;
        [SerializeField] private int colorTolerance;
        private Vector2 lastColors;

        [Header("Movement")]
        [SerializeField] private int movementTolerance;
        private int lastDistance;


        private bool answerState = false;

        private void Start()
        {
            game.ColorIndicationUpdate.AddListener(UpdateLedColors);

            serial.PortName = serialPort;
            serial.BaudRate = baudRate;

            // Guarantee that the newline is common across environments.
            serial.NewLine = "\n";
            // Once configured, the serial communication must be opened just like a file : the OS handles the communication.
            serial.Open();
        }

        private void Update()
        {
            if (!serial.IsOpen) return;
            if (serial.BytesToRead <= 0) return;

            byte[] header = SerialRead(2);
            int size = header[1];
            byte[] incomingData = SerialRead(size);

            switch ((char)header[0])
            {
                case 'v':
                    short volume = BitConverter.ToInt16(incomingData, 0);
                    //Debug.Log($"_V: {volume}")

                    UpdateAudioMixerVolume((float)volume / 1023);
                    break;

                case 'm':
                    short distance = BitConverter.ToInt16(incomingData, 0);
                    //Debug.Log($"_D Distance: {distance} ; {(float)(distance - 512) / 512}")

                    if (distance < lastDistance - movementTolerance || distance > lastDistance + movementTolerance)
                    {
                        lastDistance = distance;
                        game.MoveCard(Vector2.zero, new Vector2((float)(distance - 512) / 512 * game.GetMaxDistance(), 0));
                    }
                    break;

                case 'a':
                    short answer = BitConverter.ToInt16(incomingData, 0);
                    //Debug.Log($"_A Answer: {answer}")

                    if (!answerState && answer == 0)
                    {
                        answerState = true;
                    }
                    else if (answerState && answer == 1)
                    {
                        game.OnMoveRelease(Vector2.zero, new Vector2((float)(lastDistance - 512) / 512 * game.GetMaxDistance(), 0));
                        answerState = false;
                    }
                    break;


                default:
                    Debug.LogWarning($"Header Code {(char)header[0]} unknown");
                    break;
            }
        }

        byte[] SerialRead(int bytesToRead)
        {
            byte[] incomingData = new byte[bytesToRead];
            var bytesRead = 0;
            while (bytesRead < bytesToRead)
            {
                bytesRead += serial.Read(incomingData, bytesRead, bytesToRead - bytesRead);
            }

            return incomingData;
        }

        private void UpdateLedColors(float notFoundColorA, float foundColorA)
        {
            if (!serial.IsOpen) return;

            if (notFoundColorA * colorFactor < lastColors.x - colorTolerance || notFoundColorA * colorFactor > lastColors.x + colorTolerance
                || foundColorA * colorFactor < lastColors.y - colorTolerance || foundColorA * colorFactor > lastColors.y + colorTolerance)
            {
                lastColors = new Vector2 { x = notFoundColorA * colorFactor, y = foundColorA * colorFactor };

                byte[] notFoundAMessage = BitConverter.GetBytes((Int16)(notFoundColorA * colorFactor));
                byte[] foundAMessage = BitConverter.GetBytes((Int16)(foundColorA * colorFactor));

                Debug.Log($"Serialize: {(Int16)(notFoundColorA * colorFactor)}, {(Int16)(foundColorA * colorFactor)}");
                if (notFoundAMessage.Length != foundAMessage.Length)
                {
                    Debug.LogError($"Different Sizes: {notFoundAMessage.Length} != {foundAMessage.Length}");
                }

                serial.Write("l");
                serial.Write(new[] { (byte)notFoundAMessage.Length }, 0, 1);

                serial.Write(notFoundAMessage, 0, notFoundAMessage.Length);
                serial.Write(foundAMessage, 0, foundAMessage.Length);
            }
        }

        private void UpdateAudioMixerVolume(float volume)
        {
            if (volume < volumeMinValue)
            {
                volume = volumeMinValue;
            }
            audioMixer.SetFloat(audioMixerParameter, 20f * Mathf.Log10(volume));
        }
        
        private void OnDestroy()
        {
            if (!serial.IsOpen) { return; }
            serial.Close();
        }
    }
}