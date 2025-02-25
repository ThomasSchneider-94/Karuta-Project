using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

namespace Karuta
{
    #region Json Objects
    [Serializable]
    public class ConfigData
    {
        public string serverIP;
    }
    #endregion Json Objects

    public abstract class Downloader : MonoBehaviour
    {
        [SerializeField] protected int downloadTimeout;

        [Header("Waiting Screen")]
        [SerializeField] protected GameObject waitingPanel;
        [SerializeField] protected TextMeshProUGUI downloadingDeckTextMesh1;
        [SerializeField] protected TextMeshProUGUI downloadingDeckTextMesh2;

        protected bool connexionError = false;
        protected bool downloadFail = false;

        public static string ConfifFile { get; } = "config";
        protected string serverIP;

        virtual protected void Awake()
        {
            waitingPanel.SetActive(false);

            serverIP = JsonUtility.FromJson<ConfigData>(Resources.Load<TextAsset>(ConfifFile).text).serverIP;
        }

        /// <summary>
        /// Download all type of files
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="file"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        protected IEnumerator FileDownloader(string filePath, string endPoint)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(serverIP + endPoint);
            webRequest.downloadHandler = new DownloadHandlerFile(filePath);
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success && File.Exists(filePath))
            {
                downloadFail = true;
                File.Delete(filePath);
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                yield return ExitOnConnexionError("Connexion Error");
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                downloadFail = true;
                Debug.LogWarning(serverIP + endPoint + " file does not exist");
            }
        }

        protected IEnumerator StringDownloader(string endPoint, Action<string> onSuccess)
        {
            // Initiate the request
            using UnityWebRequest webRequest = UnityWebRequest.Get(serverIP + endPoint);
            webRequest.timeout = downloadTimeout;

            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onSuccess.Invoke(webRequest.downloadHandler.text);
            }
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                yield return ExitOnConnexionError("Connexion Error");
            }
            else
            {
                downloadFail = true;
                Debug.LogWarning(serverIP + endPoint + " does not exist");
            }
        }

        virtual protected IEnumerator ExitOnConnexionError(string error)
        {
            Debug.LogWarning("Connexion Error");
            
            downloadFail = true;
            connexionError = true;
            downloadingDeckTextMesh1.text = error;

            yield return new WaitForSeconds(2);

            waitingPanel.SetActive(false);
        }
    }
}