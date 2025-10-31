using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Web2Wave
{
    public class Web2WaveWebView : MonoBehaviour
    {
        private string _url;
        private bool _allowBackNavigation;
        private IWeb2WaveWebListener _listener;
        private Color _backgroundColor = Color.white;
        private bool _isOpen = false;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void OpenWebView(string url, string backgroundColor);
        
        [DllImport("__Internal")]
        private static extern void CloseWebView();
        
        [DllImport("__Internal")]
        private static extern void RegisterWebViewMessageCallback(string gameObjectName, string callbackName);
#elif UNITY_ANDROID || UNITY_IOS
        private AndroidJavaObject _webViewPlugin;
        private const string WebViewPluginClass = "com.web2wave.webview.WebViewPlugin";
#endif

        public static Web2WaveWebView Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OpenWebPage(
            string webPageURL,
            IWeb2WaveWebListener listener = null,
            bool allowBackNavigation = false,
            Color? backgroundColor = null)
        {
            if (!Web2Wave.Shared.IsInitialized)
            {
                throw new Exception("You must initialize apiKey before use");
            }

            if (!Web2WaveHelpers.IsValidUrl(webPageURL))
            {
                throw new Exception("You must provide valid url");
            }

            _url = webPageURL;
            _listener = listener;
            _allowBackNavigation = allowBackNavigation;
            _backgroundColor = backgroundColor ?? Color.white;
            _isOpen = true;

            // Calculate safe area offsets (simplified for Unity)
            int topOffset = 0; // Can be calculated from Screen.safeArea
            int bottomOffset = 0;

            string preparedUrl = Web2WaveHelpers.PrepareUrl(webPageURL, topOffset, bottomOffset);

#if UNITY_WEBGL && !UNITY_EDITOR
            RegisterWebViewMessageCallback(gameObject.name, "OnWebViewMessage");
            OpenWebView(preparedUrl, ColorUtility.ToHtmlStringRGB(_backgroundColor));
#elif UNITY_ANDROID || UNITY_IOS
            OpenNativeWebView(preparedUrl);
#else
            Debug.LogWarning("WebView is not supported on this platform. Opening URL in browser.");
            Application.OpenURL(preparedUrl);
#endif
        }

#if UNITY_ANDROID || UNITY_IOS
        private void OpenNativeWebView(string url)
        {
            try
            {
                using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _webViewPlugin = new AndroidJavaObject(WebViewPluginClass, currentActivity);
                    _webViewPlugin.Call("openWebView", url, _allowBackNavigation);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to open native webview: {e.Message}");
            }
        }
#endif

        public void CloseWebPage()
        {
            if (!_isOpen) return;

            _isOpen = false;

#if UNITY_WEBGL && !UNITY_EDITOR
            CloseWebView();
#elif UNITY_ANDROID || UNITY_IOS
            try
            {
                _webViewPlugin?.Call("closeWebView");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to close webview: {e.Message}");
            }
#endif

            _listener?.OnClose(null);
            _listener = null;
        }

        // Called from JavaScript/WebView
        public void OnWebViewMessage(string message)
        {
            try
            {
                var data = JsonUtility.FromJson<WebViewMessage>(message);
                
                if (data != null)
                {
                    if (data.eventName == "Quiz finished")
                    {
                        _listener?.OnQuizFinished(ParseData(data.data));
                    }
                    else if (data.eventName == "Close webview")
                    {
                        CloseWebPage();
                    }
                    else
                    {
                        _listener?.OnEvent(data.eventName, ParseData(data.data));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse webview message: {e.Message}");
            }
        }

        private Dictionary<string, object> ParseData(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
                return null;

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            }
            catch
            {
                return null;
            }
        }

        [Serializable]
        private class WebViewMessage
        {
            public string eventName;
            public string data;
        }
    }
}
