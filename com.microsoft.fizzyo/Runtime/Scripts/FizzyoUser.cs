// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Text;

//https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
#if UNITY_UWP
using Windows.Security.Authentication.Web;
using Windows.Data.Json;
using System.Net.Http;
using System.Threading.Tasks;
#endif


namespace Fizzyo
{
    // Serializable that holds user data, access token and expiry
    [System.Serializable]
    public class AllUserData
    {
        public string accessToken;
        public string expiresIn;
        public UserData user;
    }

    // Serializable that holds user data
    [System.Serializable]
    public class UserData
    {
        public string id;
        public string firstName;
        public string lastName;
        public string role;
    }
    // Serializable which holds user tag
    [System.Serializable]
    public class UserTag
    {
        public string gamerTag;
    }

    public enum LoginReturnType { SUCCESS, INCORRECT, FAILED_TO_CONNECT }
    public enum UserTagReturnType { SUCCESS, NOT_SET, FAILED_TO_CONNECT, BANNED_TAG }
    public enum CalibrationReturnType { SUCCESS, NOT_SET, FAILED_TO_CONNECT }

    /// <summary>
    /// Class that handles correct identification of each user, thanks to the use of Windows Live authentication
    /// </summary>
    public class FizzyoUser
    {
        //Client ID refers to the Fizzyo app ID, which is used to get the Windows Live auth code
        const string scopes = "wl.offline_access wl.signin wl.phone_numbers wl.emails";
        const string authorizationEndpoint = "https://login.live.com/oauth20_authorize.srf";

        private string userID;
        private string patientRecordId;
        private string token;

        /// <summary>
        /// Indicates whether someone is logged in or not 
        /// </summary>
        public bool LoggedIn { get; internal set; }
        /// <summary>
        /// String holding the username of the account logging in
        /// </summary>
        public string username;
        /// <summary>
        /// Testing variables, by default, username should be : test-patient
        /// </summary>
        public string testUsername = "test-patient";
        /// <summary>
        /// Testing variables, by default, password should be : FizzyoTesting2017
        /// </summary>
        public string testPassword = "FizzyoTesting2017";

        public string UserID { get; internal set; }
        public string AccessToken { get; internal set; }

        private bool userTagSet = false;

        public bool UserTagSet
        {
            get
            {
                return userTagSet;
            }

            set
            {
                userTagSet = value;
            }
        }

        private bool calibrationSet = false;

        public bool CalibrationSet
        {
            get
            {
                return calibrationSet;
            }

            set
            {
                calibrationSet = value;
            }
        }

#if !UNITY_EDITOR
        private bool loginInProgress = false;
#endif

        /// <summary>
        /// Method that begins the login process.
        /// Only used when logging in without the hub.
        /// </summary>
        public LoginReturnType LoginMSA(string GameID, string ApiPath)
        {
#if UNITY_UWP

             UnityEngine.WSA.Application.InvokeOnUIThread(
            async () =>
            {
                LoginAsync(GameID, ApiPath);
            }, true);

            while(loginInProgress){}
            return FizzyoNetworking.loginResult;

#elif UNITY_EDITOR
            return PostAuthentication(testUsername, testPassword);
#else
            return FizzyoNetworking.loginResult;
#endif
        }

        /// <summary>
        /// Login using the pre-authenticated Hub session using it's credentials for all networking calls
        /// </summary>
        /// <param name="userId">User ID passed from the HUb login session</param>
        /// <param name="accessToken">MSAL access token passed from the hub login session</param>
        public void LoginUsingHub(string userId,string accessToken)
        {
            UserID = userId;
            AccessToken = accessToken;
            LoggedIn = true;
            FizzyoNetworking.loginResult = LoginReturnType.SUCCESS;
        }

        /// <summary>
        /// Logs out the user. TO BE IMPLEMENTED
        /// </summary>
        public void Logout()
        {
            LoggedIn = false;
            UserID = "";
            AccessToken = "";
        }

        /// <summary>
        /// Uses a username and password to access the Fizzyo API and load in the users access token and user Id
        /// This is currently incomplete as it does not use Windows live authorization
        /// </summary>
        private LoginReturnType PostAuthentication(string username, string password)
        {
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("username", username);
            formData.Add("password", password);

            var webRequest = FizzyoNetworking.PostWebRequest(FizzyoNetworking.ApiEndpoint + "auth/test-token", formData);
            webRequest.SendWebRequest();

            while (!webRequest.isDone) { }

            if (webRequest.error != null)
            {
                return LoginReturnType.INCORRECT;
            }

            AllUserData allData = JsonUtility.FromJson<AllUserData>(webRequest.downloadHandler.text);
            UserID = allData.user.id;
            AccessToken = allData.accessToken;
            LoggedIn = true;
            return LoginReturnType.SUCCESS;
        }

#if UNITY_UWP

        public async Task LoginAsync(string GameID, string ApiPath)
        {
            const string clientID = "65973b85-c34f-41a8-a4ad-00529d1fc23c"; 
            
            string authorizationRequest = String.Format("{0}?client_id={1}&scope={2}&response_type=code&redirect_uri={3}",
                authorizationEndpoint,
                clientID,
                scopes,
                System.Uri.EscapeDataString(ApiPath + "auth-example"));

            Uri StartUri = new Uri(authorizationRequest);
            Uri EndUri = new Uri(FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + "auth-example");

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
                    FizzyoNetworking.loginResult = LoginReturnType.SUCCESS;
                    LoggedIn = true;
                    loginInProgress = false;
                    return;
                }
                else
                {
                    FizzyoNetworking.loginResult = LoginReturnType.INCORRECT;
                    loginInProgress = false;
                    return;
                }
            }

            FizzyoNetworking.loginResult = LoginReturnType.FAILED_TO_CONNECT;
            loginInProgress = false;
            return;
        }
        
        //TODO: look at adding Password vault : https://docs.microsoft.com/en-us/windows/uwp/security/credential-locker
        //https://www.wintellect.com/single-sign-on-with-oauth-in-windows-store-apps/

        public async Task<bool> RequestAccessToken(string code)
        {
            // Builds the Token request
            string tokenRequestBody = string.Format("authCode={0}&redirectUri={1}",
                    code,
                Uri.EscapeDataString(FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + "auth-example")
                );

            StringContent content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", " Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");

            HttpResponseMessage response = await client.PostAsync(FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + FizzyoNetworking.ApiEndpoint + "auth/token", content);
            string responseString = await response.Content.ReadAsStringAsync();

            try
            {
                //https://chrisbitting.com/2016/05/02/parsing-json-data-in-c-json-net-linq-httpclient/
                JsonObject jsonResponse = JsonObject.Parse(responseString);
                userID = (string)jsonResponse.GetNamedObject("user").GetNamedString("id");
                patientRecordId = (string)jsonResponse.GetNamedObject("user").GetNamedString("patientRecordId");
                token = (string)jsonResponse.GetNamedString("accessToken");
            }catch(Exception e)
            {
                //exception such as no patient record in Json found
                return false;
            }
            //patientRecordId = (string)joResponse["user"]["patientRecordId"];
            //string token = (string)joResponse["accessToken"];

            if (!response.IsSuccessStatusCode)
            {

                return false;
            }
            else
            {
                // Sets the Authentication header of our HTTP client using the acquired access token.
                //JsonObject tokens = JsonObject.Parse(responseString);
                //accessToken = tokens.GetNamedString("accessToken");
                UserID = userID;
                AccessToken = token;
                LoggedIn = true;
                return true;
            }
        }
#endif

        /// <summary>
        /// Function that runs the methods responsible for loading user tags and calibration data. 
        /// </summary>
        public void Load()
        {
            switch (LoadUserTag())
            {
                case UserTagReturnType.NOT_SET | UserTagReturnType.FAILED_TO_CONNECT:
                    UserTagSet = false;
                    break;
                case UserTagReturnType.SUCCESS:
                    UserTagSet = true;
                    break;
            }

            switch (LoadCalibrationData())
            {
                case CalibrationReturnType.NOT_SET | CalibrationReturnType.FAILED_TO_CONNECT:
                    CalibrationSet = false;
                    break;
                case CalibrationReturnType.SUCCESS:
                    CalibrationSet = true;
                    break;
            }
        }

        /// <summary>
        /// Loads in the users tag
        /// </summary>
        public UserTagReturnType LoadUserTag()
        {
            if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return UserTagReturnType.FAILED_TO_CONNECT;
            }

            //https://api.fizzyo-ucl.co.uk/api/v1/users/:id

            var webRequest = FizzyoNetworking.GetWebRequest(FizzyoNetworking.ApiEndpoint + "users/" + FizzyoFramework.Instance.User.UserID);
            webRequest.SendWebRequest();

            while (!webRequest.isDone) { }

            if (webRequest.error != null)
            {
                return UserTagReturnType.FAILED_TO_CONNECT;
            }


            UserTag allData = JsonUtility.FromJson<UserTag>(webRequest.downloadHandler.text);

            if (Regex.IsMatch(allData.gamerTag, "^[A-Z]{3}$"))
            {
                PlayerPrefs.SetInt("tagDone", 1);
                return UserTagReturnType.SUCCESS;
            }
            else
            {
                return UserTagReturnType.NOT_SET;
            }
        }

        /// <summary>
        /// Uploads a player tag to the Fizzyo API
        /// </summary>
        /// <returns>
        /// String - "Tag Upload Complete" - If upload completes  
        /// String - "Tag Upload Failed" - If upload fails
        /// String - "Please Select A Different Tag" - If tag contains profanity
        /// </returns> 
        public UserTagReturnType PostUserTag(string tag)
        {
            if (PlayerPrefs.GetInt("online") == 0)
            {
                return UserTagReturnType.FAILED_TO_CONNECT;
            }

            string[] tagFilter = { "ASS", "FUC", "FUK", "FUQ", "FUX", "FCK", "COC", "COK", "COQ", "KOX", "KOC", "KOK", "KOQ", "CAC", "CAK", "CAQ", "KAC", "KAK", "KAQ", "DIC", "DIK", "DIQ", "DIX", "DCK", "PNS", "PSY", "FAG", "FGT", "NGR", "NIG", "CNT", "KNT", "SHT", "DSH", "TWT", "BCH", "CUM", "CLT", "KUM", "KLT", "SUC", "SUK", "SUQ", "SCK", "LIC", "LIK", "LIQ", "LCK", "JIZ", "JZZ", "GAY", "GEY", "GEI", "GAI", "VAG", "VGN", "SJV", "FAP", "PRN", "LOL", "JEW", "JOO", "GVR", "PUS", "PIS", "PSS", "SNM", "TIT", "FKU", "FCU", "FQU", "HOR", "SLT", "JAP", "WOP", "KIK", "KYK", "KYC", "KYQ", "DYK", "DYQ", "DYC", "KKK", "JYZ", "PRK", "PRC", "PRQ", "MIC", "MIK", "MIQ", "MYC", "MYK", "MYQ", "GUC", "GUK", "GUQ", "GIZ", "GZZ", "SEX", "SXX", "SXI", "SXE", "SXY", "XXX", "WAC", "WAK", "WAQ", "WCK", "POT", "THC", "VAJ", "VJN", "NUT", "STD", "LSD", "POO", "AZN", "PCP", "DMN", "ORL", "ANL", "ANS", "MUF", "MFF", "PHK", "PHC", "PHQ", "XTC", "TOK", "TOC", "TOQ", "MLF", "RAC", "RAK", "RAQ", "RCK", "SAC", "SAK", "SAQ", "PMS", "NAD", "NDZ", "NDS", "WTF", "SOL", "SOB", "FOB", "SFU", "PEE", "DIE", "BUM", "BUT", "IRA" };

            if (tagFilter.Contains(tag) || !Regex.IsMatch(tag, "^[A-Z]{3}$"))
            {
                return UserTagReturnType.BANNED_TAG;
            }

            if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return UserTagReturnType.FAILED_TO_CONNECT;
            }

            var webRequest = FizzyoNetworking.PostWebRequest(FizzyoNetworking.ApiEndpoint + "users/" + FizzyoFramework.Instance.User.UserID + "/gamer-tag", null);
            webRequest.SendWebRequest();

            while (!webRequest.isDone) { }

            if (webRequest.error != null)
            {
                return UserTagReturnType.FAILED_TO_CONNECT;
            }

            PlayerPrefs.SetInt("tagDone", 1);
            PlayerPrefs.SetString("userTag", tag);

            return UserTagReturnType.SUCCESS;
        }

        /// <summary>
        /// Loads in the users calibration data
        /// </summary>
        private CalibrationReturnType LoadCalibrationData()
        {
            if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return CalibrationReturnType.FAILED_TO_CONNECT;
            }

            //https://api.fizzyo-ucl.co.uk/api/v1/users/<userId>/calibration
            var webRequest = FizzyoNetworking.GetWebRequest(FizzyoNetworking.ApiEndpoint + "users/" + FizzyoFramework.Instance.User.UserID + "/calibration");
            webRequest.SendWebRequest();

            while (!webRequest.isDone) { }

            if (webRequest.error != null)
            {
                return CalibrationReturnType.FAILED_TO_CONNECT;
            }

            CalibrationData allData = JsonUtility.FromJson<CalibrationData>(webRequest.downloadHandler.text);
            Debug.Log(JsonUtility.ToJson(allData));

            PlayerPrefs.SetInt("calLoaded", 1);

            if ((allData.pressure == 0) || (allData.time == 0))
            {
                return CalibrationReturnType.NOT_SET;

            }
            else
            {
                Debug.Log(allData.pressure);
                Debug.Log(allData.time);

                PlayerPrefs.SetInt("calDone", 1);
                PlayerPrefs.SetFloat("calPressure", allData.pressure);
                PlayerPrefs.SetFloat("calTime", allData.time);

                return CalibrationReturnType.SUCCESS;
            }
        }

        /// <summary>
        /// Uploads a players calibration data and also sets the values in player prefs
        /// </summary>
        /// <returns>
        /// String - "Upload Complete" - If upload completes  
        /// String - "Upload Failed" - If upload fails
        /// </returns> 
        public CalibrationReturnType Calibration(float pressure, float time)
        {
            PlayerPrefs.SetFloat("calPressure", pressure);
            PlayerPrefs.SetFloat("calTime", time);
            PlayerPrefs.SetInt("calDone", 1);

            if (PlayerPrefs.GetInt("online") == 0)
            {
                return CalibrationReturnType.FAILED_TO_CONNECT;
            }

            //string uploadCal = FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + "/api/v1/users/" + FizzyoFramework.Instance.User.UserID + "/calibration";

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            TimeSpan diff = DateTime.UtcNow - origin;
            int calibratedOn = (int)Math.Floor(diff.TotalSeconds);

            if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return CalibrationReturnType.FAILED_TO_CONNECT;
            }

            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("calibratedOn", calibratedOn.ToString());
            formData.Add("pressure", pressure.ToString());
            formData.Add("time", time.ToString());

            var webRequest = FizzyoNetworking.PostWebRequest(FizzyoNetworking.ApiEndpoint + "users/" + FizzyoFramework.Instance.User.UserID + "/calibration", formData);
            webRequest.SendWebRequest();

            while (!webRequest.isDone) { }

            if (webRequest.error != null)
            {
                return CalibrationReturnType.FAILED_TO_CONNECT;
            }

            return CalibrationReturnType.SUCCESS;
        }

        /// <summary>
        /// Uploads a players session data and achievements
        /// </summary>
        /// <param name="goodBreathCount"> 
        /// Integer that contains the amount of good breaths that were completed in the session
        /// </param>  
        /// <param name="badBreathCount"> 
        /// Integer that contains the amount of bad breaths that were completed in the session
        /// </param>  
        /// <param name="score"> 
        /// Integer that holds the players score for that session
        /// </param>  
        /// <param name="startTime"> 
        /// Integer that holds the time that the session was started
        /// </param>  
        /// <param name="setCount"> 
        /// Integer that holds the amount of sets that were completed in the session
        /// </param>  
        /// <param name="breathCount"> 
        /// Integer that holds the amount of breaths that were completed in the session
		/// </param>
        /// <returns>
        /// String - "Session Upload Complete /nAchievement Upload Complete" - If session upload completes and achievement upload completes
		///
        /// String - "Session Upload Complete /nAchievement Upload Failed" - If session upload completes and achievement upload fails
		///
        /// String - "Session Upload Failed /nAchievement Upload Complete" - If session upload fails and achievement upload completes
		///
        /// String - "Session Upload Failed /nAchievement Upload Failed" - If session upload fails and achievement upload fails
        /// </returns>
        public static string Session(int goodBreathCount, int badBreathCount, int score, int startTime, int setCount, int breathCount)
        {
            if (PlayerPrefs.GetInt("online") == 0 || FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return "Session Upload Failed";
            }

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            TimeSpan diff = DateTime.UtcNow - origin;
            int endTime = (int)Math.Floor(diff.TotalSeconds);

            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("id", FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID);
            formData.Add("secret", FizzyoFramework.Instance.FizzyoConfigurationProfile.GameSecret);
            formData.Add("userId", FizzyoFramework.Instance.User.UserID);
            formData.Add("setCount", setCount.ToString());
            formData.Add("breathCount", breathCount.ToString());
            formData.Add("goodBreathCount", goodBreathCount.ToString());
            formData.Add("badBreathCount", badBreathCount.ToString());
            formData.Add("score", score.ToString());
            formData.Add("startTime", startTime.ToString());
            formData.Add("endTime", endTime.ToString());

            var webRequest = FizzyoNetworking.PostWebRequest(FizzyoNetworking.ApiEndpoint + "game/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/sessions", formData);
            webRequest.SendWebRequest();

            string status = "Session Upload Complete";

            while (!webRequest.isDone) { }

            if (webRequest.error != null)
            {
                status = "Session Upload Failed";
            }

            return status;
        }
    }
}