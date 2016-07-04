using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;

namespace TaskNOW
{
    static class Authentication
    {
        public static async Task<string> Authenticate()
        {
            string state = "";
            byte[] randBytes = new byte[20];
            (new Random()).NextBytes(randBytes);
            for (int i = 0; i < 20; i++)
            {
                state += randBytes[i].ToString();
            }
            var oauthUrl = "https://todoist.com/oauth/authorize";
            var clientID = ApiCodes.ClientID;
            var scope = "task:add,data:read";
            var redirect = "https://localhost/oauth2callback";
            var requestUri = new Uri($"{oauthUrl}?client_id={clientID}&scope={scope}&state={state}");
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
                                responseState = rpValue;
                                break;
                            case "state":
                                responseCode = rpValue;
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
    }
}
