using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;
using System.Collections;
using Karuta.Objects;
using UnityEngine.TestTools;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Karuta
{
    #region Json Objects
    [Serializable]
    public class ConfigData
    {
        public string serverIP;
    }
    #endregion Json Objects

    public class LoadManager : MonoBehaviour
    {
        public static LoadManager Instance { get; private set; }

        [Header("General")]
        [SerializeField] private int downloadTimeout;
        [SerializeField] private int connexionCheckTimeout;

        [Header("Streaming Parameters")]
        [SerializeField] private int minDownloadedBytes = 2048;

        [Header("Default")]
        [SerializeField] private Sprite defaultSprite;

        [Header("Directories")]
        [SerializeField] private string decksFile = "DecksInfo.json";
        [SerializeField] private string categoriesFile = "Categories.json";
        [SerializeField] private string decksDirectory = "Decks";
        [SerializeField] private string categoryVisualsDirectory = "Categories Visual";
        [SerializeField] private string coversDirectory = "Covers";
        [SerializeField] private string visualsDirectory = "Visuals";
        [SerializeField] private string audioDirectory = "Audio";

        [Header("End Points")]
        [SerializeField] private string deckNamesEp = "deck_names";
        [SerializeField] private string deckDataEp = "deck_metadata/";
        [SerializeField] private string categoriesAndTypesEp = "get_categories";
        [SerializeField] private string categoriesIconEp = "get_categories";
        [SerializeField] private string coversEp = "cover/";
        [SerializeField] private string visualsEp = "visual/";
        [SerializeField] private string audioEp = "sound/";

        // Files
        #region Files
        public static string DecksFilePath { get; private set; }

        public static string CategoriesFilePath { get; private set; }
        #endregion Files

        // Directory paths
        #region Directory Paths
        public static string DecksDirectoryPath { get; private set; }
        public static string CategoryVisualsDirectoryPath { get; private set; }
        public static string CoversDirectoryPath { get; private set; }
        public static string VisualsDirectoryPath { get; private set; }
        public static string AudioDirectoryPath { get; private set; }
        public static string ThemesDirectoryPath { get; private set; }
        #endregion Directory Paths

        // End Points
        #region End Points
        public static string DeckNamesEndPoint { get; private set; }
        public static string DeckDataEndPoint { get; private set; }
        public static string CategoriesAndTypesEndPoint { get; private set; }
        public static string CategoriesIconEndPoint { get; private set; }
        public static string CoversEndPoint { get; private set; }
        public static string VisualsEndPoint { get; private set; }
        public static string AudioEndPoint { get; private set; }
        #endregion End Points

        public UnityEvent LoadManagerInitializedEvent { get; } = new();

        private string serverIP;

        private bool connexionError = false;

        private Coroutine audioCoroutine;

        private void Awake()
        {
            Instance = this;

            Initialize();
        }

        private void Initialize()
        {
            InitializeStaticPaths();

            serverIP = JsonUtility.FromJson<ConfigData>(Resources.Load<TextAsset>("config").text).serverIP;

            InitDirectories();
        }

        private void InitializeStaticPaths()
        {
            // Files
            DecksFilePath = Path.Combine(Application.persistentDataPath, decksFile);
            CategoriesFilePath = Path.Combine(Application.persistentDataPath, categoriesFile);

            // Directories
            DecksDirectoryPath = Path.Combine(Application.persistentDataPath, decksDirectory);
            CategoryVisualsDirectoryPath = Path.Combine(Application.persistentDataPath, categoryVisualsDirectory);
            CoversDirectoryPath = Path.Combine(Application.persistentDataPath, coversDirectory);
            VisualsDirectoryPath = Path.Combine(Application.persistentDataPath, visualsDirectory);
            AudioDirectoryPath = Path.Combine(Application.persistentDataPath, audioDirectory);
            ThemesDirectoryPath = Path.Combine(Application.persistentDataPath, "Themes");

            // End Points
            DeckNamesEndPoint = deckNamesEp;
            DeckDataEndPoint = deckDataEp;
            CategoriesAndTypesEndPoint = categoriesAndTypesEp;
            CategoriesIconEndPoint = categoriesIconEp;
            CoversEndPoint = coversEp;
            VisualsEndPoint = visualsEp;
            AudioEndPoint = audioEp;
        }

        private static void InitDirectories()
        {
            if (!Directory.Exists(CategoryVisualsDirectoryPath))
            {
                Directory.CreateDirectory(CategoryVisualsDirectoryPath);
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
        /// <summary>
        /// Load a category visual from file
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        public Sprite LoadCategoryVisualSprite(string visual)
        {
            return LoadSprite(Path.Combine(CategoriesFilePath, visual));
        }

        /// <summary>
        /// Load a deck cover from file
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>
        public Sprite LoadCoverSprite(string cover)
        {
            return LoadSprite(Path.Combine(CoversDirectoryPath, cover));
        }

        /// <summary>
        /// Load a card visual from file
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        public Sprite LoadVisualSprite(string visual)
        {
            return LoadSprite(Path.Combine(VisualsDirectoryPath, visual));
        }

        /// <summary>
        /// Load a sprite from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="alreadyChecked"></param>
        /// <returns></returns>
        private Sprite LoadSprite(string filePath, bool alreadyChecked = false)
        {
            if (!alreadyChecked && !File.Exists(filePath))
            {
                return defaultSprite;
            }

            Texture2D texture = LoadTexture(filePath, true);

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Load a theme texture from file
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D LoadThemeTexture(string texture)
        {
            return LoadTexture(Path.Combine(ThemesDirectoryPath, texture));
        }

        /// <summary>
        /// Load a texture from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="alreadyChecked"></param>
        /// <returns></returns>
        private static Texture2D LoadTexture(string filePath, bool alreadyChecked = false)
        {
            if (!alreadyChecked && !File.Exists(filePath))
            {
                return null;
            }

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            Texture2D texture = new(1, 1);
            texture.LoadImage(bytes);

            return texture;
        }

        #region Visual
        /// <summary>
        /// Load a visual by its name, from files or download. Call onLoaded at the end.
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="onLoaded"></param>
        public void LoadVisual(string visual, Action<Sprite, bool> onLoaded)
        {
            if (File.Exists(Path.Combine(VisualsDirectoryPath, visual)))
            {
                Debug.Log("From visual file : " + visual);
                onLoaded.Invoke(LoadSprite(visual, true), true);
            }
            else if (!connexionError)
            {
                Debug.Log("Download visual: " + visual);
                StartCoroutine(DownloadVisual(visual, onLoaded));
            }
            else
            {
                onLoaded.Invoke(defaultSprite, false);
            }
        }

        /// <summary>
        /// Load a visual by its name from download. Call onLoaded at the end.
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="onLoaded"></param>
        /// <returns></returns>
        public IEnumerator DownloadVisual(string visual, Action<Sprite, bool> onLoaded)
        {
            // Initiate the webRequest
            using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(serverIP + VisualsEndPoint + visual);
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Get texture from the response
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                // Create a new Sprite from the downloaded texture
                onLoaded?.Invoke(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)), true);
            }
            else
            {
                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    EnableConnexionError();
                    Debug.LogWarning("Connexion Error");
                }
                Debug.LogWarning("Failed to download visual: " + webRequest.error);
                onLoaded?.Invoke(defaultSprite, false);
            }
        }
        #endregion Visual

        #region Audio
        /// <summary>
        /// Stream an audio by its name, from files or download. Call onLoaded at the end.
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="onLoaded"></param>
        public void LoadAudio(string audio, Action<AudioClip> onLoaded)
        {
            if (audioCoroutine != null)
            {
                StopCoroutine(audioCoroutine);
            }
            
            if (File.Exists(Path.Combine(AudioDirectoryPath, audio)))
            {
                Debug.Log("From audio file : " + audio);
                audioCoroutine = StartCoroutine(LoadAudioFromFile(audio, onLoaded));
            }
            else if (!connexionError)
            {
                Debug.Log("Download audio : " + audio);
                audioCoroutine = StartCoroutine(DownloadAudio(audio, onLoaded));
            }
            else
            {
                onLoaded.Invoke(CreateSilentAudioClip());
            }
        }

        /// <summary>
        /// Stream an audio by its name from files. Call onLoaded at the end.
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="onLoaded"></param>
        /// <returns></returns>
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
                onLoaded?.Invoke(CreateSilentAudioClip());
            }

            // Continue downloading the rest of the audio
            while (!fileRequest.isDone)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Load an audio by its name from *download. Call onLoaded at the end.
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="onLoaded"></param>
        /// <returns></returns>
        public IEnumerator DownloadAudio(string audio, Action<AudioClip> onLoaded)
        {
            // Initiate the webRequest
            using UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(serverIP + AudioEndPoint + audio, AudioType.MPEG);
            ((DownloadHandlerAudioClip)webRequest.downloadHandler).streamAudio = true;
            webRequest.timeout = downloadTimeout;

            webRequest.SendWebRequest();

            float t = 0f;
            while (webRequest.downloadedBytes < (ulong)minDownloadedBytes * 10 && t < downloadTimeout)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (t < downloadTimeout)
            {
                if (((DownloadHandlerAudioClip)webRequest.downloadHandler).audioClip != null)
                {
                    onLoaded?.Invoke(((DownloadHandlerAudioClip)webRequest.downloadHandler).audioClip);

                    // Continue downloading the rest of the audio
                    while (!webRequest.isDone)
                    {
                        yield return null;
                    }

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                        {
                            EnableConnexionError();
                        }
                        Debug.LogWarning("Error streaming audio: " + webRequest.error);
                    }
                }
                else
                {
                    onLoaded?.Invoke(CreateSilentAudioClip());
                }
            }
            else
            {
                Debug.LogWarning("Connexion Error: Failed to load audio");
                onLoaded?.Invoke(CreateSilentAudioClip());
                EnableConnexionError();
            }
        }

        /// <summary>
        /// Create a silent audioClip
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        private static AudioClip CreateSilentAudioClip(float duration = 90f)
        {
            int sampleRate = 44100; // Standard audio sample rate
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount]; // All zeroes for silence

            AudioClip silentClip = AudioClip.Create("SilentClip", sampleCount, 1, sampleRate, false);
            silentClip.SetData(samples, 0);

            return silentClip;
        }
        #endregion Audio

        #endregion Loader

        #region Connexion Error
        /// <summary>
        /// Enable the check for connexion error
        /// </summary>
        private void EnableConnexionError()
        {
            Debug.LogWarning("Enable Connexion Error");
            if (!connexionError)
            {
                connexionError = true;
                StartCoroutine(ConnectionCheck());
            }
        }

        /// <summary>
        /// Check regularly if the connexion if now up
        /// </summary>
        /// <returns></returns>
        private IEnumerator ConnectionCheck()
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(serverIP + "deck_names");
            webRequest.timeout = connexionCheckTimeout;

            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                connexionError = false;
                Debug.LogWarning("End Connexion Error");
            }
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                yield return new WaitForSeconds(connexionCheckTimeout);

                Debug.LogWarning("Connexion Error Again");
                StartCoroutine(ConnectionCheck());
            }
        }
        #endregion Connexion Error

        public Sprite GetDefaultSprite()
        {
            return defaultSprite;
        }

        public string GetServerIP()
        {
            return serverIP;
        }
    }
}