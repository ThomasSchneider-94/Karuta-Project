using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Threading.Tasks;
using static Karuta.ScriptableObjects.JsonObjects;
using System.Net;
using System;
using System.Collections;

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
        public static string DecksDirectoryPath { get; private set; }
        public static string CoversDirectoryPath { get; private set; }
        public static string VisualsDirectoryPath { get; private set; }
        public static string AudioDirectoryPath { get; private set; }
        public static string ThemesDirectoryPath { get; private set; }

        public UnityEvent DirectoriesInitializedEvent { get; } = new UnityEvent();

        private string serverIP;

        private bool initialized;

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

        #region Visual
        public void LoadVisual(string visual, Action<Sprite> onLoaded)
        {
            if (File.Exists(Path.Combine(VisualsDirectoryPath, visual)))
            {
                Debug.Log("From visual file : " + visual);
                LoadSpriteFromFile(visual, onLoaded);
            }
            else
            {
                Debug.Log("Download visual: " + visual);
                StartCoroutine(DownloadVisual(visual, onLoaded));
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
            // Initiate the request
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
                Debug.LogError($"Failed to download visual: {webRequest.error}");
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
                Debug.Log("From audio file : " + audio);
                StartCoroutine(LoadAudioFromFile(audio, onLoaded));
            }
            else
            {
                Debug.Log("Download audio : " + audio);
                StartCoroutine(DownloadAudio(audio, onLoaded));
            }
        }

        public IEnumerator LoadAudioFromFile(string audio, Action<AudioClip> onLoaded)
        {
            yield return StartCoroutine(LoadAudioFromPath("file://" + Path.Combine(AudioDirectoryPath, audio), onLoaded));
        }

        public IEnumerator DownloadAudio(string audio, Action<AudioClip> onLoaded)
        {
            yield return StartCoroutine(LoadAudioFromPath(serverIP + "sound/" + audio, onLoaded));
        }

        public IEnumerator LoadAudioFromPath(string audioPath, Action<AudioClip> onLoaded)
        {
            // Initiate the request
            using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.MPEG);
            ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = true;
            request.timeout = downloadTimeout;

            request.SendWebRequest();

            while (request.downloadedBytes < (ulong)minDownloadedBytes * 10)
            {
                yield return null;
            }

            if (((DownloadHandlerAudioClip)request.downloadHandler).audioClip != null)
            {
                onLoaded?.Invoke(((DownloadHandlerAudioClip)request.downloadHandler).audioClip);
            }
            else
            {
                onLoaded?.Invoke(defaultAudio);
            }
            
            // Continue downloading the rest of the audio
            while (!request.isDone)
            {
                yield return null;
            }

            // Check for errors
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error streaming audio: {request.error}");
            }
        }
        #endregion Audio

        #endregion Loader

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