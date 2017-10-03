using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Windows.Data.Json;

//https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
#if UNITY_UWP
using SimpleJSON;
using Windows.Security.Authentication.Web;
#endif

namespace Fizzyo
{
    public class FizzyoUser : MonoBehaviour
    {

        //Client ID refers to the Fizzyo app ID, which is used to get the Windows Live auth code
        const string clientID = "65973b85-c34f-41a8-a4ad-00529d1fc23c";
        const string scopes = "wl.offline_access wl.signin wl.phone_numbers wl.emails";
        const string authorizationEndpoint = "https://login.live.com/oauth20_authorize.srf";
        const string redirectURI = "https://api.fizzyo-ucl.co.uk/auth-example";
        const string tokenEndpoint = "https://api.fizzyo-ucl.co.uk/api/v1/auth/token";

        public bool loggedIn = false;
        public string username;
        public string name;


         public async Task<bool> Login()
        {

            #if UNITY_UWP
            
            string authorizationRequest = String.Format("{0}?client_id={1}&scope={2}&response_type=code&redirect_uri={3}",
                authorizationEndpoint,
                PlayerPrefs.GetString("gameId"),
                //state,
                scopes,
                System.Uri.EscapeDataString(redirectURI)
);

            Uri StartUri = new Uri(authorizationRequest);
            Uri EndUri = new Uri(redirectURI);

            WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, StartUri, EndUri);

            if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                Uri authorizationResponse = new Uri(WebAuthenticationResult.ResponseData.ToString());
                string queryString = authorizationResponse.Query;
                Dictionary<string, string> queryStringParams =
                    queryString.Substring(1).Split('&')
                         .ToDictionary(c => c.Split('=')[0],
                                       c => Uri.UnescapeDataString(c.Split('=')[1]));
                // Gets the Authorization code 
                String code = queryStringParams["code"];

                // Authorization Code is now ready to use!


               
                bool tokenExhanged = await RequestAccessToken(code);

                if (tokenExhanged == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
          
            
        #endif

        }




        public async Task<bool> RequestAccessToken(string code)
        {
            // Builds the Token request
            string tokenRequestBody = string.Format("authCode={0}&redirectUri={1}",
                    code,
                Uri.EscapeDataString(redirectURI)
                );

            StringContent content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", " Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");

            HttpResponseMessage response = await client.PostAsync(tokenEndpoint, content);
            string responseString = await response.Content.ReadAsStringAsync();

                //https://chrisbitting.com/2016/05/02/parsing-json-data-in-c-json-net-linq-httpclient/
                JsonObject joResponse = JsonObject.Parse(responseString);
            userID = (string)joResponse["user"]["id"];
            patientRecordId = (string)joResponse["user"]["patientRecordId"];
            string token = (string)joResponse["accessToken"];

            if (!response.IsSuccessStatusCode)
            {

                return false;
            }
            else
            {
                // Sets the Authentication header of our HTTP client using the acquired access token.
                JsonObject tokens = JsonObject.Parse(responseString);
                accessToken = tokens.GetNamedString("accessToken");
                
                return true;

            }

        }



    }
}
