using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Karuta.Menu;

namespace Karuta
{
    public class LoadManager : MonoBehaviour
    {
        public static LoadManager Instance { get; private set; }
        [SerializeField] private int downloadTimeout;

        [Header("General")]
        [SerializeField] private int connexionCheckTimeout;

        [Header("Streaming Parameters")]
        [SerializeField] private int minDownloadedBytes = 2048;

        [Header("Default")]
        [SerializeField] private Sprite defaultSprite;

        private bool connexionError = false;
        private string serverIP;

        public static string DecksFilePath { get; private set; } = "DecksInfo.json";
        public static string CategoriesFilePath { get; private set; } = "Categories.json";
        public static string DecksDirectoryPath { get; private set; } = "Decks";
        public static string CategoryVisualsDirectoryPath { get; private set; } = "Categories_Visual";
        public static string CoversDirectoryPath { get; private set; } = "Covers";
        public static string VisualsDirectoryPath { get; private set; } = "Visuals";
        public static string AudioDirectoryPath { get; private set; } = "Audio";
        public static string ThemesDirectoryPath { get; private set; } = "Themes";

        private Coroutine audioCoroutine;

        private void Awake()
        {
            Instance = this;

            serverIP = JsonUtility.FromJson<ConfigData>(Resources.Load<TextAsset>(Downloader.ConfifFile).text).serverIP;

            InitPathes();

            InitDirectories();
        }

        private static void InitPathes()
        {
            // Files
            DecksFilePath = Path.Combine(Application.persistentDataPath, DecksFilePath);
            CategoriesFilePath = Path.Combine(Application.persistentDataPath, CategoriesFilePath);

            // Directories
            DecksDirectoryPath = Path.Combine(Application.persistentDataPath, DecksDirectoryPath);
            CategoryVisualsDirectoryPath = Path.Combine(Application.persistentDataPath, CategoryVisualsDirectoryPath);
            CoversDirectoryPath = Path.Combine(Application.persistentDataPath, CoversDirectoryPath);
            VisualsDirectoryPath = Path.Combine(Application.persistentDataPath, VisualsDirectoryPath);
            AudioDirectoryPath = Path.Combine(Application.persistentDataPath, AudioDirectoryPath);
            ThemesDirectoryPath = Path.Combine(Application.persistentDataPath, ThemesDirectoryPath);
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
        /// Load a Category Visual from file
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        public Sprite LoadCategoryVisualFromFile(string visual)
        {
            return LoadSpriteFromFile(Path.Combine(CategoriesFilePath, visual));
        }

        /// <summary>
        /// Load a Deck Cover from file
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>
        public Sprite LoadCoverFromFile(string cover)
        {
            return LoadSpriteFromFile(Path.Combine(CoversDirectoryPath, cover));
        }

        /// <summary>
        /// Load a theme texture from file
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D LoadThemeTextureFromFile(string texture)
        {
            return LoadTextureFromFile(Path.Combine(ThemesDirectoryPath, texture));
        }

        #region Visual
        /// <summary>
        /// Load a Visual by its name, from files or download. Call onLoaded at the end.
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="onLoaded"></param>
        public void LoadCardVisual(string visual, Action<Sprite, bool> onLoaded)
        {
            if (File.Exists(Path.Combine(VisualsDirectoryPath, visual)))
            {
                Debug.Log("From visual file : " + visual);
                onLoaded.Invoke(LoadCardVisualFromFile(visual), true);
            }
            else if (!connexionError)
            {
                Debug.Log("Download visual: " + visual);
                StartCoroutine(DownloadVisualSprite(visual, onLoaded));
            }
            else
            {
                onLoaded.Invoke(defaultSprite, false);
            }
        }

        /// <summary>
        /// Load a card Visual from file
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        public Sprite LoadCardVisualFromFile(string visual)
        {
            return LoadSpriteFromFile(Path.Combine(VisualsDirectoryPath, visual));
        }

        /// <summary>
        /// Load a Visual by its name from download. Call onLoaded at the end.
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="onLoaded"></param>
        /// <returns></returns>
        public IEnumerator DownloadVisualSprite(string visual, Action<Sprite, bool> onLoaded)
        {
            // Initiate the webRequest
            using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(serverIP + DeckContentManager.VisualEndPoint + visual);
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
        public void LoadCardAudio(string audio, Action<AudioClip> onLoaded)
        {
            if (audioCoroutine != null)
            {
                StopCoroutine(audioCoroutine);
            }
            
            if (File.Exists(Path.Combine(AudioDirectoryPath, audio)))
            {
                Debug.Log("From audio file : " + audio);
                audioCoroutine = StartCoroutine(LoadCardAudioFromFile(audio, onLoaded));
            }
            else if (!connexionError)
            {
                Debug.Log("Download audio : " + audio);
                audioCoroutine = StartCoroutine(DownloadCardAudio(audio, onLoaded));
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
        public IEnumerator LoadCardAudioFromFile(string audio, Action<AudioClip> onLoaded)
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
        public IEnumerator DownloadCardAudio(string audio, Action<AudioClip> onLoaded)
        {
            // Initiate the webRequest
            using UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(serverIP + DeckContentManager.AudioEndPoint + audio, AudioType.MPEG);
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

        /// <summary>
        /// Load a texture from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="alreadyChecked"></param>
        /// <returns></returns>
        private static Texture2D LoadTextureFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            Texture2D texture = new(1, 1);
            texture.LoadImage(bytes);

            return texture;
        }

        /// <summary>
        /// Load a sprite from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="alreadyChecked"></param>
        /// <returns></returns>
        private Sprite LoadSpriteFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return defaultSprite;
            }

            Texture2D texture = LoadTextureFromFile(filePath);

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
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
            using UnityWebRequest webRequest = UnityWebRequest.Get(serverIP + UpdateDecks.DeckNamesEndPoint);
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
    }
}