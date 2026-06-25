using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Web2Wave
{
    public class Web2Wave : MonoBehaviour
    {
        private static Web2Wave _instance;
        private const string BaseURL = "https://api.web2wave.com";
        private string _apiKey;

        public static Web2Wave Shared
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("Web2Wave");
                    _instance = go.AddComponent<Web2Wave>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public bool IsInitialized => !string.IsNullOrEmpty(_apiKey);

        private Dictionary<string, string> Headers
        {
            get
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    throw new Exception("You must initialize apiKey before use");
                }
                return new Dictionary<string, string>
                {
                    { "api-key", _apiKey },
                    { "Cache-Control", "no-cache" },
                    { "Pragma", "no-cache" },
                    { "platform", GetPlatform() },
                    { "screen_size", GetScreenSize() },
                    { "timezone", GetTimezone() },
                    { "os_version", GetOSVersion() }
                };
            }
        }

        private static string GetPlatform()
        {
#if UNITY_IOS
            return "iOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "Other";
#endif
        }

        private static string GetScreenSize()
        {
            return $"{Screen.width}x{Screen.height}";
        }

        private static string GetTimezone()
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var totalMinutes = (int)offset.TotalMinutes;
            var hours = totalMinutes / 60;
            var minutes = Math.Abs(totalMinutes % 60);
            var sign = hours >= 0 ? "+" : "-";
            return $"UTC{sign}{Math.Abs(hours):D2}:{minutes:D2}";
        }

        private static string GetOSVersion()
        {
#if UNITY_IOS
            return $"iOS {UnityEngine.iOS.Device.systemVersion}";
#elif UNITY_ANDROID
            return $"Android {SystemInfo.operatingSystem}";
#else
            return SystemInfo.operatingSystem;
#endif
        }

        public void Initialize(string apiKey)
        {
            _apiKey = apiKey;
        }

        private IEnumerator FetchSubscriptionStatusCoroutine(
            string web2waveUserId,
            System.Action<SubscriptionResponse> onSuccess,
            System.Action onError)
        {
            string url = $"{BaseURL}/api/user/subscriptions?user={Uri.EscapeDataString(web2waveUserId)}";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                foreach (var header in Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        SubscriptionResponse response = JsonConvert.DeserializeObject<SubscriptionResponse>(request.downloadHandler.text);
                        onSuccess?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to parse subscription response: {e.Message}");
                        onError?.Invoke();
                    }
                }
                else
                {
                    Debug.LogError($"Failed to fetch subscriptions: {request.error}");
                    onError?.Invoke();
                }
            }
        }

        public void FetchSubscriptions(string web2waveUserId, System.Action<List<SubscriptionData>> onSuccess, System.Action<string> onError)
        {
            StartCoroutine(FetchSubscriptionStatusCoroutine(
                web2waveUserId,
                response =>
                {
                    onSuccess?.Invoke(response?.subscription);
                },
                () => onError?.Invoke("Failed to fetch subscriptions")));
        }

        public void HasActiveSubscription(string web2waveUserId, System.Action<bool> onSuccess, System.Action<string> onError)
        {
            StartCoroutine(FetchSubscriptionStatusCoroutine(
                web2waveUserId,
                response =>
                {
                    if (response?.subscription != null)
                    {
                        bool hasActive = response.subscription.Exists(sub => sub.status == "active" || sub.status == "trialing");
                        onSuccess?.Invoke(hasActive);
                    }
                    else
                    {
                        onSuccess?.Invoke(false);
                    }
                },
                () => onError?.Invoke("Failed to check subscription status")));
        }

        public void ChargeUser(string web2waveUserId, int priceId, System.Action<Web2WaveResponse> onComplete)
        {
            StartCoroutine(ChargeUserCoroutine(web2waveUserId, priceId, onComplete));
        }

        private IEnumerator ChargeUserCoroutine(string web2waveUserId, int priceId, System.Action<Web2WaveResponse> onComplete)
        {
            string url = $"{BaseURL}/api/subscription/user/charge";
            
            var formData = new Dictionary<string, string>
            {
                { "user_id", web2waveUserId },
                { "price_id", priceId.ToString() }
            };

            using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
            {
                foreach (var header in Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                
                string body = Web2WaveHelpers.ToQueryString(formData);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                        bool isSuccess = result.ContainsKey("success") && result["success"]?.ToString() == "1";
                        onComplete?.Invoke(new Web2WaveResponse(isSuccess));
                    }
                    catch
                    {
                        onComplete?.Invoke(new Web2WaveResponse(false, "Failed to parse response"));
                    }
                }
                else
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                        string errorMsg = result.ContainsKey("message") ? result["message"].ToString() : request.error;
                        onComplete?.Invoke(new Web2WaveResponse(false, errorMsg));
                    }
                    catch
                    {
                        onComplete?.Invoke(new Web2WaveResponse(false, request.error));
                    }
                }
            }
        }

        public void CancelSubscription(string paySystemId, string comment, System.Action<Web2WaveResponse> onComplete)
        {
            StartCoroutine(CancelSubscriptionCoroutine(paySystemId, comment, onComplete));
        }

        private IEnumerator CancelSubscriptionCoroutine(string paySystemId, string comment, System.Action<Web2WaveResponse> onComplete)
        {
            string url = $"{BaseURL}/api/subscription/cancel";
            
            var formData = new Dictionary<string, string>
            {
                { "pay_system_id", paySystemId }
            };
            
            if (!string.IsNullOrEmpty(comment))
            {
                formData["comment"] = comment;
            }

            using (UnityWebRequest request = UnityWebRequest.Put(url, ""))
            {
                foreach (var header in Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                
                string body = Web2WaveHelpers.ToQueryString(formData);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                        bool isSuccess = result.ContainsKey("success") && result["success"]?.ToString() == "1";
                        onComplete?.Invoke(new Web2WaveResponse(isSuccess));
                    }
                    catch
                    {
                        onComplete?.Invoke(new Web2WaveResponse(false, "Failed to parse response"));
                    }
                }
                else
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                        string errorMsg = result.ContainsKey("error_msg") ? result["error_msg"].ToString() : request.error;
                        onComplete?.Invoke(new Web2WaveResponse(false, errorMsg));
                    }
                    catch
                    {
                        onComplete?.Invoke(new Web2WaveResponse(false, request.error));
                    }
                }
            }
        }

        public void RefundSubscription(string paySystemId, string invoiceId, string comment, System.Action<Web2WaveResponse> onComplete)
        {
            StartCoroutine(RefundSubscriptionCoroutine(paySystemId, invoiceId, comment, onComplete));
        }

        private IEnumerator RefundSubscriptionCoroutine(string paySystemId, string invoiceId, string comment, System.Action<Web2WaveResponse> onComplete)
        {
            string url = $"{BaseURL}/api/subscription/refund";
            
            var formData = new Dictionary<string, string>
            {
                { "pay_system_id", paySystemId },
                { "invoice_id", invoiceId }
            };
            
            if (!string.IsNullOrEmpty(comment))
            {
                formData["comment"] = comment;
            }

            using (UnityWebRequest request = UnityWebRequest.Put(url, ""))
            {
                foreach (var header in Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                
                string body = Web2WaveHelpers.ToQueryString(formData);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                        bool isSuccess = result.ContainsKey("success") && result["success"]?.ToString() == "1";
                        onComplete?.Invoke(new Web2WaveResponse(isSuccess));
                    }
                    catch
                    {
                        onComplete?.Invoke(new Web2WaveResponse(false, "Failed to parse response"));
                    }
                }
                else
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                        string errorMsg = result.ContainsKey("error_msg") ? result["error_msg"].ToString() : request.error;
                        onComplete?.Invoke(new Web2WaveResponse(false, errorMsg));
                    }
                    catch
                    {
                        onComplete?.Invoke(new Web2WaveResponse(false, request.error));
                    }
                }
            }
        }

        public void FetchUserProperties(string web2waveUserId, System.Action<Dictionary<string, string>> onSuccess, System.Action<string> onError)
        {
            StartCoroutine(FetchUserPropertiesCoroutine(web2waveUserId, onSuccess, onError));
        }

        private IEnumerator FetchUserPropertiesCoroutine(
            string web2waveUserId,
            System.Action<Dictionary<string, string>> onSuccess,
            System.Action<string> onError)
        {
            string url = $"{BaseURL}/api/user/properties?user={Uri.EscapeDataString(web2waveUserId)}";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                foreach (var header in Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        UserPropertiesResponse response = JsonConvert.DeserializeObject<UserPropertiesResponse>(request.downloadHandler.text);
                        Dictionary<string, string> properties = new Dictionary<string, string>();
                        
                        if (response?.properties != null)
                        {
                            foreach (var prop in response.properties)
                            {
                                properties[prop.property] = prop.value;
                            }
                        }
                        
                        onSuccess?.Invoke(properties);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to parse user properties: {e.Message}");
                        onError?.Invoke("Failed to parse response");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to fetch user properties: {request.error}");
                    onError?.Invoke(request.error);
                }
            }
        }

        public void UpdateUserProperty(string web2waveUserId, string property, string value, System.Action<Web2WaveResponse> onComplete)
        {
            StartCoroutine(UpdateUserPropertyCoroutine(web2waveUserId, property, value, onComplete));
        }

        private IEnumerator UpdateUserPropertyCoroutine(
            string web2waveUserId,
            string property,
            string value,
            System.Action<Web2WaveResponse> onComplete)
        {
            string url = $"{BaseURL}/api/user/properties?user={Uri.EscapeDataString(web2waveUserId)}";
            
            var payload = new Dictionary<string, string>
            {
                { "property", property },
                { "value", value }
            };
            
            string jsonBody = JsonConvert.SerializeObject(payload);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                foreach (var header in Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
                request.SetRequestHeader("Content-Type", "application/json");
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                        bool isSuccess = result.ContainsKey("result") && result["result"]?.ToString() == "1";
                        onComplete?.Invoke(new Web2WaveResponse(isSuccess));
                    }
                    catch
                    {
                        onComplete?.Invoke(new Web2WaveResponse(false, "Failed to parse response"));
                    }
                }
                else
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                        string errorMsg = result.ContainsKey("error_msg") ? result["error_msg"].ToString() : request.error;
                        onComplete?.Invoke(new Web2WaveResponse(false, errorMsg));
                    }
                    catch
                    {
                        onComplete?.Invoke(new Web2WaveResponse(false, request.error));
                    }
                }
            }
        }

        public void SetRevenuecatProfileID(string web2waveUserId, string revenuecatProfileId, System.Action<Web2WaveResponse> onComplete)
        {
            UpdateUserProperty(web2waveUserId, "revenuecat_profile_id", revenuecatProfileId, onComplete);
        }

        public void SetAdaptyProfileID(string web2waveUserId, string adaptyProfileId, System.Action<Web2WaveResponse> onComplete)
        {
            UpdateUserProperty(web2waveUserId, "adapty_profile_id", adaptyProfileId, onComplete);
        }

        public void SetQonversionProfileID(string web2waveUserId, string qonversionProfileId, System.Action<Web2WaveResponse> onComplete)
        {
            UpdateUserProperty(web2waveUserId, "qonversion_profile_id", qonversionProfileId, onComplete);
        }

        public void Identify(System.Action<IdentifyResponse> onSuccess, System.Action<string> onError)
        {
            StartCoroutine(IdentifyCoroutine(onSuccess, onError));
        }

        private IEnumerator IdentifyCoroutine(System.Action<IdentifyResponse> onSuccess, System.Action<string> onError)
        {
            string url = $"{BaseURL}/api/user/identify";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                foreach (var header in Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        IdentifyResponse response = JsonConvert.DeserializeObject<IdentifyResponse>(request.downloadHandler.text);
                        onSuccess?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to parse identify response: {e.Message}");
                        onError?.Invoke("Failed to parse response");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to identify user: {request.error}");
                    onError?.Invoke(request.error);
                }
            }
        }
    }
}
