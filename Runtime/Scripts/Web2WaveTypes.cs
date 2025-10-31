using System;
using System.Collections.Generic;

namespace Web2Wave
{
    [Serializable]
    public class Web2WaveResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

        public Web2WaveResponse(bool isSuccess, string errorMessage = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }

    [Serializable]
    public class SubscriptionData
    {
        public string status;
        public string id;
        // Add other subscription fields as needed
    }

    [Serializable]
    public class SubscriptionResponse
    {
        public List<SubscriptionData> subscription;
    }

    [Serializable]
    public class UserProperty
    {
        public string property;
        public string value;
    }

    [Serializable]
    public class UserPropertiesResponse
    {
        public List<UserProperty> properties;
    }

    public interface IWeb2WaveWebListener
    {
        void OnEvent(string eventName, Dictionary<string, object> data = null);
        void OnClose(Dictionary<string, object> data = null);
        void OnQuizFinished(Dictionary<string, object> data = null);
    }
}
