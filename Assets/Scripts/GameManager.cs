using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Karuta.ScriptableObjects;
using static Karuta.ScriptableObjects.JsonObjects;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor.PackageManager.Requests;

namespace Karuta
{
    public class GameManager : MonoBehaviour
    {
        

        public static GameManager Instance { get; private set; }

        // Theme Manager
        private readonly List<LightJsonTheme> themes = new();
        private JsonTheme currentTheme;



        // Events
        #region Events
        public UnityEvent GameManagerInitializedEvent { get; } = new UnityEvent();
        public UnityEvent UpdateThemeEvent { get; } = new UnityEvent();
        #endregion Events

        private bool initialized = false;

        private void Awake()
        {
            Debug.Log("Awake");

            // Be sure that there is only one instance of GameManager
            if (Instance == null)
            {
                Instance = this;

                initialized = true;
            }
            else
            {
                Destroy(gameObject); // Destroy if another GameManager exist
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Debug.Log("Invoke Initialize");
            GameManagerInitializedEvent.Invoke();
        }

        public bool IsInitialized()
        {
            return initialized;
        }
    }
}