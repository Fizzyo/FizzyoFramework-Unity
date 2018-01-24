	using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;

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
    public enum CalibrationReturnType { SUCCESS, NOT_SET, FAILED_TO_CONNECT}

	/// <summary>
	/// Class that handles correct identification of each user, thanks to the use of Windows Live authentication
	/// </summary>
    public class FizzyoUser 
    {

        //Client ID refers to the Fizzyo app ID, which is used to get the Windows Live auth code
        const string clientID = "65973b85-c34f-41a8-a4ad-00529d1fc23c"; //ce680d1e-27dc-4ffa-bc74-200d79a9e702
        const string scopes = "wl.offline_access wl.signin wl.phone_numbers wl.emails";
        const string authorizationEndpoint = "https://login.live.com/oauth20_authorize.srf";
        const string redirectURI = "https://api.fizzyo-ucl.co.uk/auth-example";
        const string tokenEndpoint = "https://api.fizzyo-ucl.co.uk/api/v1/auth/token";

        private string userID;
        private string patientRecordId;
        private string token;

		/// <summary>
		/// Indicates whether someone is logged in or not 
		/// </summary>
        public bool loggedIn = false;
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


        private bool loginInProgress = false;
        private LoginReturnType loginResult = LoginReturnType.FAILED_TO_CONNECT;
        private bool userTagSet;
        private bool calibrationSet;

		/// <summary>
        /// Method that begins the login process.
        /// </summary>
        public LoginReturnType Login()
        {

#if UNITY_UWP
            loginInProgress = true;
             UnityEngine.WSA.Application.InvokeOnUIThread(
            async () =>
            {
                LoginAsync();
            }, true);

            while(loginInProgress){}
            return loginResult;
            

#elif UNITY_EDITOR
            return PostAuthentication(testUsername, testPassword);
#else
       return loginResult;

#endif

        }

		/// <summary>
        /// Logs out the user. TO BE IMPLEMENTED
        /// </summary>
        public void Logout()
        {

        }



        /// <summary>
        /// Uses a username and password to access the Fizzyo API and load in the users access token and user Id
        /// This is currently incomplete as it does not use Windows live authorization
        /// </summary>
        private LoginReturnType PostAuthentication(string username, string password)
        {

            string postAuth = "https://api.fizzyo-ucl.co.uk/api/v1/auth/test-token";


            WWWForm form = new WWWForm();
            form.AddField("username", username);
            form.AddField("password", password);
            WWW sendPostAuth = new WWW(postAuth, form);

            while (!sendPostAuth.isDone) { }

            if (sendPostAuth.error != null)
            {
                return LoginReturnType.INCORRECT;
            }
            

            AllUserData allData = JsonUtility.FromJson<AllUserData>(sendPostAuth.text);
            UserID = allData.user.id;
            AccessToken = allData.accessToken;

            
            return LoginReturnType.SUCCESS;
            


        }




#if UNITY_UWP




        public async Task LoginAsync()
        {

            string authorizationRequest = String.Format("{0}?client_id={1}&scope={2}&response_type=code&redirect_uri={3}",
                authorizationEndpoint,
                clientID,
                //state,
                scopes,
                System.Uri.EscapeDataString(redirectURI));

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
                       loginResult =  LoginReturnType.SUCCESS;
                        loginInProgress = false;
                        return;
                }
                else
                {
                    loginResult =  LoginReturnType.INCORRECT;
                    loginInProgress = false;
                    return;
                }
            }

            loginResult =  LoginReturnType.FAILED_TO_CONNECT;
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
                loggedIn = true;
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
                    userTagSet = false;
                    break;
                case UserTagReturnType.SUCCESS:
                    userTagSet = true;
                    break;
            }


            switch (LoadCalibrationData())
            {
                case CalibrationReturnType.NOT_SET | CalibrationReturnType.FAILED_TO_CONNECT:
                    calibrationSet = false;
                    break;
                case CalibrationReturnType.SUCCESS:
                    calibrationSet = true;
                    break;
            }


        }
        /// <summary>
        /// Loads in the users tag
        /// </summary>
        public UserTagReturnType LoadUserTag()
        {

            //https://api.fizzyo-ucl.co.uk/api/v1/users/:id

            string getTag = "https://api.fizzyo-ucl.co.uk/api/v1/users/" + PlayerPrefs.GetString("userId");

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));
            WWW sendGetTag = new WWW(getTag, null, headers);

            while (!sendGetTag.isDone) { }

            if (sendGetTag.error != null)
            {
                return UserTagReturnType.FAILED_TO_CONNECT;
            }


            UserTag allData = JsonUtility.FromJson<UserTag>(sendGetTag.text);

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

            string uploadTag = "https://api.fizzyo-ucl.co.uk/api/v1/users/" + PlayerPrefs.GetString("userId") + "/gamer-tag";

            WWWForm form = new WWWForm();
            form.AddField("gamerTag", tag);
            Dictionary<string, string> headers = form.headers;
            headers["Authorization"] = "Bearer " + PlayerPrefs.GetString("accessToken");

            byte[] rawData = form.data;

            WWW sendPostUnlock = new WWW(uploadTag, rawData, headers);

            while (!sendPostUnlock.isDone) { }

            if (sendPostUnlock.error != null)
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
            //https://api.fizzyo-ucl.co.uk/api/v1/users/<userId>/calibration

            string getCal = "https://api.fizzyo-ucl.co.uk/api/v1/users/" + PlayerPrefs.GetString("userId") + "/calibration";

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));
            WWW sendGetCal = new WWW(getCal, null, headers);

            while (!sendGetCal.isDone) { }

            if (sendGetCal.error != null)
            {
                return CalibrationReturnType.FAILED_TO_CONNECT;
            }

            CalibrationData allData = JsonUtility.FromJson<CalibrationData>(sendGetCal.text);
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

            string uploadCal = "https://api.fizzyo-ucl.co.uk/api/v1/users/" + PlayerPrefs.GetString("userId") + "/calibration";

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            TimeSpan diff = DateTime.UtcNow - origin;
            int calibratedOn = (int)Math.Floor(diff.TotalSeconds);

            WWWForm form = new WWWForm();
            form.AddField("calibratedOn", calibratedOn);
            form.AddField("pressure", pressure.ToString());
            form.AddField("time", time.ToString());
            Dictionary<string, string> headers = form.headers;
            headers["Authorization"] = "Bearer " + PlayerPrefs.GetString("accessToken");

            byte[] rawData = form.data;

            WWW sendPostUnlock = new WWW(uploadCal, rawData, headers);

            while (!sendPostUnlock.isDone) { }

            if (sendPostUnlock.error != null)
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

            if (PlayerPrefs.GetInt("online") == 0)
            {
                return "Session Upload Failed";
            }

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            TimeSpan diff = DateTime.UtcNow - origin;
            int endTime = (int)Math.Floor(diff.TotalSeconds);

            string postSession = "https://api.fizzyo-ucl.co.uk/api/v1/game/:id/sessions";

            WWWForm form = new WWWForm();

            form.AddField("id", PlayerPrefs.GetString("gameId"));
            form.AddField("secret", PlayerPrefs.GetString("gameSecret"));
            form.AddField("userId", PlayerPrefs.GetString("userId"));
            form.AddField("setCount", setCount);
            form.AddField("breathCount", breathCount);
            form.AddField("goodBreathCount", goodBreathCount);
            form.AddField("badBreathCount", badBreathCount);
            form.AddField("score", score);
            form.AddField("startTime", startTime);
            form.AddField("endTime", endTime);

            Dictionary<string, string> headers = form.headers;
            headers["Authorization"] = "Bearer " + PlayerPrefs.GetString("accessToken");

            byte[] rawData = form.data;

            WWW sendPostSession = new WWW(postSession, rawData, headers);

            string status = "Session Upload Complete";

            while (!sendPostSession.isDone) { }

            if (sendPostSession.error != null)
            {
                status = "Session Upload Failed";
            }


            return status;

        }


    }
}
