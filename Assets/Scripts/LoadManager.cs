using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;
using System.Collections;
using Karuta.Objects;
using System.Net;

namespace Karuta
{
    public class LoadManager : MonoBehaviour
    {
        public static LoadManager Instance { get; private set; }

        [Header("General")]
        [SerializeField] private int downloadTimeout;

        [Header("Streaming Parameters")]
        [SerializeField] private int minDownloadedBytes = 2048;

        [Header("Default")]
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private AudioClip defaultAudio;

        // Directory paths
        public static string DecksFilePath { get; private set; }
        public static string CategoriesFilePath { get; private set; }
        public static string CategoriesDirectoryPath { get; private set; }
        public static string DecksDirectoryPath { get; private set; }
        public static string CoversDirectoryPath { get; private set; }
        public static string VisualsDirectoryPath { get; private set; }
        public static string AudioDirectoryPath { get; private set; }
        public static string ThemesDirectoryPath { get; private set; }

        public UnityEvent DirectoriesInitializedEvent { get; } = new UnityEvent();

        private string serverIP;

        private bool initialized;

        private bool connexionError = false;

        private void Awake()
        {
            // Be sure that there is only one instance of DecksManager
            if (Instance == null)
            {
                Instance = this;

                Initialize();

                initialized = true;
            }
            else
            {
                Destroy(gameObject); // Destroy if another DecksManager exist
            }
            DontDestroyOnLoad(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            DirectoriesInitializedEvent.Invoke();
        }

        public bool IsInitialized()
        {
            return initialized;
        }

        private void Initialize()
        {
            DecksFilePath = Path.Combine(Application.persistentDataPath, "DecksInfo.json");
            CategoriesFilePath = Path.Combine(Application.persistentDataPath, "Categories", "Categories.json");
            CategoriesDirectoryPath = Path.Combine(Application.persistentDataPath, "Categories");
            DecksDirectoryPath = Path.Combine(Application.persistentDataPath, "Decks");
            CoversDirectoryPath = Path.Combine(Application.persistentDataPath, "Covers");
            VisualsDirectoryPath = Path.Combine(Application.persistentDataPath, "Visuals");
            AudioDirectoryPath = Path.Combine(Application.persistentDataPath, "Audio");
            ThemesDirectoryPath = Path.Combine(Application.persistentDataPath, "Themes");

            serverIP = JsonUtility.FromJson<ConfigData>(Resources.Load<TextAsset>("config").text).serverIP;

            InitDirectories();
        }

        private static void InitDirectories()
        {
            if (!Directory.Exists(CategoriesDirectoryPath))
            {
                Directory.CreateDirectory(CategoriesDirectoryPath);
            }
            if (!Directory.Exists(DecksDirectoryPath))
            {
                Directory.CreateDirectory(DecksDirectoryPath);
            }
            if (!Directory.Exists(CoversDirectoryPath))
            {
                Directory.CreateDirectory(CoversDirectoryPath);
            }
            if (!Directory.Exists(VisualsDirectoryPath))
            {
                Directory.CreateDirectory(VisualsDirectoryPath);
            }
            if (!Directory.Exists(AudioDirectoryPath))
            {
                Directory.CreateDirectory(AudioDirectoryPath);
            }
            if (!Directory.Exists(ThemesDirectoryPath))
            {
                Directory.CreateDirectory(ThemesDirectoryPath);
            }
        }

        #region Loader
        public Sprite LoadCover(string coverName)
        {
            if (!File.Exists(Path.Combine(CoversDirectoryPath, coverName)))
            {
                return defaultSprite;
            }

            string filePath = Path.Combine(CoversDirectoryPath, coverName);

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            Texture2D texture = new(1, 1);
            texture.LoadImage(bytes);

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public Sprite LoadCategoryVisual(string categoryVisual)
        {
            if (!File.Exists(Path.Combine(CategoriesDirectoryPath, categoryVisual)))
            {
                return defaultSprite;
            }

            string filePath = Path.Combine(CategoriesDirectoryPath, categoryVisual);

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            Texture2D texture = new(1, 1);
            texture.LoadImage(bytes);

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        #region Theme
        public static Sprite LoadThemeVisual(string visual)
        {
            if (!File.Exists(Path.Combine(ThemesDirectoryPath, visual)))
            {
                return null;
            }

            string filePath = Path.Combine(CoversDirectoryPath, visual);

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            Texture2D texture = new(1, 1);
            texture.LoadImage(bytes);

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        #endregion Theme

        #region Visual
        public void LoadVisual(string visual, Action<Sprite> onLoaded)
        {
            if (File.Exists(Path.Combine(VisualsDirectoryPath, visual)))
            {
                Debug.Log("From visual file : " + visual);
                LoadSpriteFromFile(visual, onLoaded);
            }
            else if (!connexionError)
            {
                Debug.Log("Download visual: " + visual);
                StartCoroutine(DownloadVisual(visual, onLoaded));
            }
            else
            {
                onLoaded.Invoke(defaultSprite);
            }
        }

        public static void LoadSpriteFromFile(string visual, Action<Sprite> onLoaded)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(Path.Combine(VisualsDirectoryPath, visual));

            Texture2D texture = new(1, 1);
            texture.LoadImage(bytes);

            onLoaded.Invoke(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
        }

        public IEnumerator DownloadVisual(string visual, Action<Sprite> onLoaded)
        {
            // Initiate the webRequest
            using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(serverIP + "visual/" + visual);
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Get texture from the response
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                // Create a new Sprite from the downloaded texture
                onLoaded?.Invoke(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
            }
            else
            {
                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    connexionError = true;
                    Debug.LogWarning("Connexion Error");
                }
                Debug.LogWarning("Failed to download visual: " + webRequest.error);
                onLoaded?.Invoke(defaultSprite);
            }
        }
        #endregion Visual

        #region Audio
        public void LoadAudio(string audio, Action<AudioClip> onLoaded)
        {
            StopAllCoroutines();
            if (File.Exists(Path.Combine(AudioDirectoryPath, audio)))
            {
                //Debug.Log("From audio file : " + audio)
                StartCoroutine(LoadAudioFromFile(audio, onLoaded));
            }
            else if (!connexionError)
            {
                //Debug.Log("Download audio : " + audio)
                StartCoroutine(DownloadAudio(audio, onLoaded));
            }
            else
            {
                onLoaded.Invoke(defaultAudio);
            }
        }

        public IEnumerator LoadAudioFromFile(string audio, Action<AudioClip> onLoaded)
        {
            // Initiate the webRequest
            using UnityWebRequest fileRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + Path.Combine(AudioDirectoryPath, audio), AudioType.MPEG);
            ((DownloadHandlerAudioClip)fileRequest.downloadHandler).streamAudio = true;
            fileRequest.timeout = downloadTimeout;

            fileRequest.SendWebRequest();

            while (fileRequest.downloadedBytes < (ulong)minDownloadedBytes * 10)
            {
                yield return null;
            }

            if (((DownloadHandlerAudioClip)fileRequest.downloadHandler).audioClip != null)
            {
                onLoaded?.Invoke(((DownloadHandlerAudioClip)fileRequest.downloadHandler).audioClip);
            }
            else
            {
                onLoaded?.Invoke(defaultAudio);
            }

            // Continue downloading the rest of the audio
            while (!fileRequest.isDone)
            {
                yield return null;
            }
        }

        public IEnumerator DownloadAudio(string audio, Action<AudioClip> onLoaded)
        {
            // Initiate the webRequest
            using UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(serverIP + "sound/" + audio, AudioType.MPEG);
            ((DownloadHandlerAudioClip)webRequest.downloadHandler).streamAudio = true;
            webRequest.timeout = downloadTimeout;

            webRequest.SendWebRequest();

            while (webRequest.downloadedBytes < (ulong)minDownloadedBytes * 10)
            {
                yield return null;
            }

            if (((DownloadHandlerAudioClip)webRequest.downloadHandler).audioClip != null)
            {
                onLoaded?.Invoke(((DownloadHandlerAudioClip)webRequest.downloadHandler).audioClip);
            }
            else
            {
                onLoaded?.Invoke(defaultAudio);
            }

            // Continue downloading the rest of the audio
            while (!webRequest.isDone)
            {
                yield return null;
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    connexionError = true;
                }
                Debug.LogWarning("Error streaming audio: " + webRequest.error);
            }
        }
        #endregion Audio

        #endregion Loader

        public void SetConnexionError(bool connexionError)
        {
            this.connexionError = connexionError;
        }

        public Sprite GetDefaultSprite()
        {
            return defaultSprite;
        }

        public AudioClip GetDefaultAudio()
        {
            return defaultAudio;
        }

        public string GetServerIP()
        {
            return serverIP;
        }
    }
}