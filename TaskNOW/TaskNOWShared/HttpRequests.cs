using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.UI.Popups;

namespace TaskNOW
{
    static class HttpRequests
    {
        private static System.Net.Http.HttpClient MainClient = new HttpClient();

        public static async Task<HttpResponseMessage> HttpPost(string baseUrl, params string[] parameters)
        {
            if (parameters.Length % 2 != 0)
                throw new ArgumentException("Parameters are name/value pairs, so the length must be even.", "parameters");
            StringBuilder uriBuilder = new StringBuilder(baseUrl);
            for (int i = 0; i < parameters.Length; i += 2)
            {
                if (i == 0)
                    uriBuilder.Append('?');
                else
                    uriBuilder.Append('&');
                uriBuilder.Append(parameters[i]);
                uriBuilder.Append('=');
                uriBuilder.Append(parameters[i + 1]);
            }
            return await MainClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()));
        }

        public static async Task<string> Authenticate()
        {
            string state = "";
            byte[] randBytes = new byte[20];
            (new Random()).NextBytes(randBytes);
            for (int i = 0; i < 20; i++)
            {
                state += randBytes[i].ToString();
            }
            var baseUrl = "https://todoist.com/oauth/authorize";
            var clientID = ApiCodes.ClientID;
            var scope = "task:add,data:read";
            var redirect = "https://localhost/oauth2callback";
            var requestUri = new Uri($"{baseUrl}?client_id={clientID}&scope={scope}&state={state}");
            var redirectUri = new Uri(redirect);
            var result = await Windows.Security.Authentication.Web.WebAuthenticationBroker.AuthenticateAsync(Windows.Security.Authentication.Web.WebAuthenticationOptions.None, requestUri, redirectUri);
            if (result.ResponseStatus == Windows.Security.Authentication.Web.WebAuthenticationStatus.Success)
            {
                if (result.ResponseData.Contains("code="))
                {
                    Uri responseUri = new Uri(result.ResponseData);
                    WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(responseUri.Query);
                    string responseState = null;
                    string responseCode = null;
                    for (int i = 0; i < decoder.Count; i++)
                    {
                        var rpName = decoder[i].Name;
                        var rpValue = decoder[i].Value;
                        switch (rpName)
                        {
                            case "code":
                                responseCode = rpValue;
                                break;
                            case "state":
                                responseState = rpValue;
                                break;
                        }
                    }
                    if (responseState != state)
                        return null;
                    else
                        return responseCode;
                }
            }
            return null;
        }

        public static async Task<string> GetAccessToken(string authenticationCode)
        {
            var baseUrl = "https://todoist.com/oauth/access_token";
            var result = await HttpPost(baseUrl, "client_id", ApiCodes.ClientID, "client_secret", ApiCodes.ClientSecret, "code", authenticationCode); // await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri));
            if (result.IsSuccessStatusCode == true)
            {
                var stringContent = await result.Content.ReadAsStringAsync();
                JsonValue root;
                JsonObject jsonObject;
                JsonValue.TryParse(stringContent, out root);
                jsonObject = root.GetObject();
                if (jsonObject.ContainsKey("access_token"))
                    return jsonObject.GetNamedString("access_token");
                
            }
            return null;
        }
    }
}
