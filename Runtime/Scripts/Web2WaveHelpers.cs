using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
