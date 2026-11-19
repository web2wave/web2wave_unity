using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Web2Wave
{
    public static class Web2WaveHelpers
    {
        public static bool IsValidUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            catch
            {
                return false;
            }
        }

        public static string PrepareUrl(string baseUrl, int topOffset, int bottomOffset)
        {
            UriBuilder uriBuilder = new UriBuilder(baseUrl);
            string query = uriBuilder.Query;
            
            // Remove leading '?' if present
            if (query.StartsWith("?"))
                query = query.Substring(1);
            
            var parameters = new Dictionary<string, string>();
            
            // Parse existing query parameters
            if (!string.IsNullOrEmpty(query))
            {
                string[] pairs = query.Split('&');
                foreach (string pair in pairs)
                {
                    string[] keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        parameters[Uri.UnescapeDataString(keyValue[0])] = Uri.UnescapeDataString(keyValue[1]);
                    }
                }
            }
            
            // Add or update our parameters
            parameters["webview_unity"] = "1";
            parameters["top_padding"] = topOffset.ToString();
            parameters["bottom_padding"] = bottomOffset.ToString();
            
            // Rebuild query string
            uriBuilder.Query = ToQueryString(parameters);
            return uriBuilder.ToString();
        }

        public static string ToQueryString(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return string.Empty;

            var query = new StringBuilder();
            bool first = true;
            foreach (var param in parameters)
            {
                if (!first)
                    query.Append("&");
                
                query.Append(Uri.EscapeDataString(param.Key));
                query.Append("=");
                query.Append(Uri.EscapeDataString(param.Value ?? ""));
                
                first = false;
            }
            
            return query.ToString();
        }

        /// <summary>
        /// Interprets a "success"/"result" flag from an API response.
        /// The backend returns it as a JSON boolean (true), a number (1) or a string ("1" / "true").
        /// </summary>
        public static bool IsSuccessValue(object value)
        {
            if (value == null)
                return false;

            if (value is JValue jValue)
                return IsSuccessValue(jValue.Value);

            if (value is JToken)
                return false; // objects / arrays are never a success flag

            if (value is bool b)
                return b;

            if (value is string str)
            {
                str = str.Trim();
                return str == "1" || string.Equals(str, "true", StringComparison.OrdinalIgnoreCase);
            }

            if (value is long || value is int || value is short || value is byte ||
                value is ulong || value is uint || value is ushort || value is sbyte)
            {
                return Convert.ToInt64(value, CultureInfo.InvariantCulture) == 1;
            }

            if (value is double || value is float || value is decimal)
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture) == 1.0;
            }

            return false;
        }

        /// <summary>
        /// Parses a response body into a JSON object, or returns null if it is empty or not a JSON object.
        /// </summary>
        public static JObject TryParseObject(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;
            try
            {
                return JToken.Parse(body) as JObject;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the backend error text: "error_msg" first, then "message", otherwise the fallback.
        /// </summary>
        public static string ExtractErrorMessage(JObject json, string fallback)
        {
            if (json != null)
            {
                foreach (var key in new[] { "error_msg", "message" })
                {
                    var token = json[key];
                    if (token != null && token.Type != JTokenType.Null)
                    {
                        var text = token.ToString();
                        if (!string.IsNullOrWhiteSpace(text))
                            return text;
                    }
                }
            }
            return fallback;
        }

        /// <summary>
        /// Returns the backend error text for a raw response body ("error_msg", then "message"), otherwise the fallback.
        /// </summary>
        public static string ExtractErrorMessage(string body, string fallback)
        {
            return ExtractErrorMessage(TryParseObject(body), fallback);
        }

        /// <summary>
        /// Builds a Web2WaveResponse for an action endpoint (charge, cancel, refund, update property).
        /// </summary>
        /// <param name="body">Raw response body (may be null or empty).</param>
        /// <param name="isHttpSuccess">True when the HTTP request succeeded (2xx).</param>
        /// <param name="httpError">Transport / HTTP error text, used when the body has no message.</param>
        /// <param name="successKey">Name of the success flag: "success", or "result" for user properties.</param>
        public static Web2WaveResponse ParseActionResponse(string body, bool isHttpSuccess, string httpError, string successKey = "success")
        {
            var json = TryParseObject(body);

            if (!isHttpSuccess)
            {
                var fallback = string.IsNullOrEmpty(httpError) ? "Request failed" : httpError;
                return new Web2WaveResponse(false, ExtractErrorMessage(json, fallback));
            }

            if (json == null)
                return new Web2WaveResponse(false, "Failed to parse response");

            if (IsSuccessValue(json[successKey]))
                return new Web2WaveResponse(true);

            return new Web2WaveResponse(false, ExtractErrorMessage(json, "Request failed"));
        }
    }
}
