using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            return await HttpPostWithJson(baseUrl, null, parameters);
        }

        private static void CreateRandIds(StringBuilder tempIdBuilder, StringBuilder uuidBuilder)
        {
            byte[] randBuffer = new byte[64];
            new Random().NextBytes(randBuffer);
            string hexadecimal = "0123456789abcdef";
            for (int i = 0; i < 64; i++)
            {
                tempIdBuilder.Append(hexadecimal[randBuffer[i] % 16]);
                uuidBuilder.Append(hexadecimal[randBuffer[i] / 16]);
            }
        }

        public static async Task<HttpStatusCode> CreateTask(int projectId, DateTime dueDate, string description, int priority, string token)
        {
            DateTime utcDueDate = dueDate.ToUniversalTime();
            StringBuilder tempIdBuilder = new StringBuilder();
            StringBuilder uuidBuilder = new StringBuilder();
            CreateRandIds(tempIdBuilder, uuidBuilder);
            var taskStringBuilder = new StringBuilder("[{\"type\": \"item_add\", \"temp_id\": \"");
            taskStringBuilder.Append(tempIdBuilder);
            taskStringBuilder.Append("\", \"uuid\": \"");
            taskStringBuilder.Append(uuidBuilder);
            taskStringBuilder.Append("\", \"args\": {\"content\": \"");
            taskStringBuilder.Append(description);
            taskStringBuilder.Append("\", \"project_id\": ");
            taskStringBuilder.Append(projectId);
            taskStringBuilder.Append(", \"priority\": ");
            taskStringBuilder.Append(priority);
            taskStringBuilder.Append(", \"due_date_utc\": ");
            taskStringBuilder.Append('"');
            //taskStringBuilder.Append(new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" }[(int)utcDueDate.DayOfWeek]);
            //taskStringBuilder.Append(' ');
            taskStringBuilder.Append(utcDueDate.ToString("yyyy-MM-ddTHH:mm"));// "dd MMM yyyy HH:mm:ss"));
            taskStringBuilder.Append('"');//" +0000\"");
            taskStringBuilder.Append("}}]");
            var resp = await HttpPost("https://todoist.com/API/v7/sync", "token", token, "commands", WebUtility.HtmlDecode(taskStringBuilder.ToString()));
            return resp.StatusCode;
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
            var scope = "data:read_write";
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

        public static async Task<HttpResponseMessage> HttpPostWithJson(string baseUrl, string json, params string[] parameters)
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
            if (json == null)
                return await MainClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()));
            else
                return await MainClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }
}
