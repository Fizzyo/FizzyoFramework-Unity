using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// This namespace contains all of the classes that handle loading and uploading data via the Fizzyo API, calibration
/// and breath framework
/// </summary>
namespace Fizzyo
{
    /// <summary>
    /// This namespace contains all of the classes that handle loading and uploading data via the Fizzyo API
    /// </summary>
    namespace Data
    {
        /// <summary>
        /// This class contains all of the functions for loading and uploading via the Fizzyo API
        /// </summary>
        public class Load : MonoBehaviour
        {
            /// <summary>
            /// Loads the user data from the Fizzyo API
            /// PlayerPrefs holds the users information in the following configuration:
            /// "online" - Integer - 0 or 1 - Tells the developer if the user is playing offline or online 
            /// "calDone" - Integer - 0 or 1 - Tells the developer if the user has completed calibration 
            /// "achievments" - String - Holds the achievements for the game and, if the user is online, tells the developer
            /// which achievements have been unlocked 
            /// "achievmentsToUpload" - String - Holds the achievements that have been unlocked in the current session
            /// Current players userID + "AchievementProgress" - String - Holds data on the achievement progress that the user has made in this game 
            /// "accessToken" - String - Holds the access token that is aquired for the current user
            /// "tagDone" - Integer - 0 or 1 - Tells the developer if the user has completed setting a tag
            /// "userTag" - String - Holds the user tag
            /// "calPressure" - Float - Holds the pressure that the user has set in their calibration
            /// "calTime" - Integer - Holds the breath length that the user has set in their calibration
            /// "userId" - String - Holds the user Id that is aquired for the current user
            /// "gameId" - String - Holds the game Id for this specific game
            /// "gameSecret" - String - Holds the game secret for this specific game
            /// "userLoaded" - Integer - 0 or 1 - Shows if the users access token was loaded
            /// "calLoaded" - Integer - 0 or 1 - Shows if the users calibration data was loaded
            /// "achLoaded" - Integer - 0 or 1 - Shows if the users achievement data was loaded
            /// "tagLoaded" - Integer - 0 or 1 - Shows if the users tag was loaded
            /// </summary>
            /// <param name="gameId"> 
            /// String that contains the current games ID
            /// </param>  
            /// <param name="gameSecret"> 
            /// String that contains the current games secret
            /// </param>  
            /// <returns>
            /// true if data is loaded and playing online
            /// false if daa is not loaded and playing offline
            /// </returns>  
            public static bool LoadUserData(string gameId, string gameSecret)
            {

                ResetPlayerPrefs();

                // Delete in lu of using windows live auth and resirectURI
                string username = "test-patient";
                string password = "FizzyoTesting2017";

                PlayerPrefs.SetString("gameId", gameId);
                PlayerPrefs.SetString("gameSecret", gameSecret);

                PostAuthentication(username, password);

                if (PlayerPrefs.GetInt("userLoaded") == 0)
                {
                    PlayOffline();
                    return false;

                } else
                {

                    GetCalibrationData(); 
                    GetUnlockedAchievements(); 
                    GetUserTag();

                    if (PlayerPrefs.GetInt("calLoaded") == 0 || PlayerPrefs.GetInt("achLoaded") == 0 || PlayerPrefs.GetInt("tagLoaded") == 0)
                    {

                        PlayOffline();
                        return false;
                    }
                    PlayerPrefs.SetInt("online", 1);
                    return true;
                }

            

            }

            /// <summary>
            /// Uses a username and password to access the Fizzyo API and load in the users access token and user Id
            /// This is currently incomplete as it does not use Windows live authorization
            /// </summary>
            private static void PostAuthentication(string username, string password)
            {

                string postAuth = "https://api.fizzyo-ucl.co.uk/api/v1/auth/test-token";

                /*
                // Using windows Live and redirect
                WWWForm form = new WWWForm();
                form.AddField("redirectUri", redirectUri);
                form.AddField("authCode", authCode);
                WWW sendPostAuth = new WWW(postAuth, form);
                
                Stopwatch s = new Stopwatch();
                s.Start();

                while (!sendPostAuth.isDone)
                {
                    if (s.Elapsed > TimeSpan.FromSeconds(5))
                    {
                        s.Stop();
                        PlayerPrefs.SetInt("userLoaded", 0);
                        PlayerPrefs.SetInt("calDone", 0);
                        return "\nAuthentication Failed";
                    }
                }

                s.Stop();

                AllUserData allData = JsonUtility.FromJson<AllUserData>(sendPostAuth.text);
                accessToken = allData.accessToken;
                userId = allData.user.id;

                */

                WWWForm form = new WWWForm();
                form.AddField("username", username);
                form.AddField("password", password);
                WWW sendPostAuth = new WWW(postAuth, form);

                while (!sendPostAuth.isDone) { }

                if (sendPostAuth.error != null)
                {
                    PlayerPrefs.SetInt("userLoaded", 0);
                    PlayerPrefs.SetInt("calDone", 0);
                    return;
                }

                AllUserData allData = JsonUtility.FromJson<AllUserData>(sendPostAuth.text);

                PlayerPrefs.SetString("accessToken", allData.accessToken);
                PlayerPrefs.SetString("userId", allData.user.id);

                PlayerPrefs.SetInt("userLoaded", 1);

            }

            /// <summary>
            /// Loads in the users calibration data
            /// </summary>
            private static void GetCalibrationData()
            {
                // https://api.fizzyo-ucl.co.uk/api/v1/users/<userId>/calibration

                string getCal = "https://api.fizzyo-ucl.co.uk/api/v1/users/" + PlayerPrefs.GetString("userId") + "/calibration";

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));
                WWW sendGetCal = new WWW(getCal, null, headers);

                while (!sendGetCal.isDone) { }

                if (sendGetCal.error != null)
                {
                    PlayerPrefs.SetInt("calLoaded", 0);
                    return;
                }

                CalibrationData allData = JsonUtility.FromJson<CalibrationData>(sendGetCal.text);
                Debug.Log(JsonUtility.ToJson(allData));

                PlayerPrefs.SetInt("calLoaded", 1);

                if ((allData.pressure == 0) || (allData.time == 0))
                {
                    PlayerPrefs.SetInt("calDone", 0);
                    return;
                }
                else
                {
                    Debug.Log(allData.pressure);
                    Debug.Log(allData.time);

                    PlayerPrefs.SetInt("calDone", 1);
                    PlayerPrefs.SetFloat("calPressure", allData.pressure);
                    PlayerPrefs.SetFloat("calTime", allData.time);
                }

            }

            /// <summary>
            /// Loads in the users unlocked achievements and achievement progress
            /// </summary>
            private static void GetUnlockedAchievements()
            {

                PlayerPrefs.SetString("achievementsToUpload", "");

                string getUnlock = "https://api.fizzyo-ucl.co.uk/api/v1/users/" + PlayerPrefs.GetString("userId") + "/unlocked-achievements/" + PlayerPrefs.GetString("gameId");

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));
                WWW sendGetUnlock = new WWW(getUnlock, null, headers);

                while (!sendGetUnlock.isDone) { }

                if (sendGetUnlock.error != null)
                {
                    PlayerPrefs.SetInt("achLoaded", 0);
                    return;
                }

                string unlockedJSONData = sendGetUnlock.text;
                AllAchievementData allUnlocked = JsonUtility.FromJson<AllAchievementData>(unlockedJSONData);

                AllAchievementData allAchievements = JsonUtility.FromJson<AllAchievementData>(PlayerPrefs.GetString("achievements"));

                string progress = PlayerPrefs.GetString(PlayerPrefs.GetString("userId") + "AchievementProgress");

                if (progress == "" || progress == null)
                {
                    PlayerPrefs.SetString(PlayerPrefs.GetString("userId") + "AchievementProgress", PlayerPrefs.GetString("achievements"));
                    progress = PlayerPrefs.GetString(PlayerPrefs.GetString("userId") + "AchievementProgress");
                }

                Debug.Log(progress);

                AllAchievementData allUserProgress = JsonUtility.FromJson<AllAchievementData>(progress);

                for (int i = 0; i < allUnlocked.unlockedAchievements.Length; i++)
                {

                    for (int j = 0; j < allAchievements.achievements.Length; j++)
                    {

                        if (allUnlocked.unlockedAchievements[i].id == allAchievements.achievements[j].id)
                        {
                            allAchievements.achievements[j].unlock = 1;
                        }

                    }

                }
                
                for (int j = 0; j < allAchievements.achievements.Length; j++)
                {

                    for (int k = 0; k < allUserProgress.achievements.Length; k++)
                    {

                        if (allUserProgress.achievements[k].id == allAchievements.achievements[j].id)
                        {
                            allAchievements.achievements[j].unlockProgress = allUserProgress.achievements[k].unlockProgress;

                        }

                    }

                }

                // string allJSONUserAchievementProgress = PlayerPrefs.GetString("achievementProgress");
                // AllAchievementData allUserAchievementProgress = JsonUtility.FromJson<AllAchievementData>(allJSONUserAchievementProgress);

                Debug.Log(PlayerPrefs.GetString(PlayerPrefs.GetString("userId") + "AchievementProgress"));

                string newAllData = JsonUtility.ToJson(allAchievements);
                PlayerPrefs.SetString("achievements", newAllData);
                PlayerPrefs.SetInt("achLoaded", 1);

            }

            /// <summary>
            /// Loads in the users tag
            /// </summary>
            private static void GetUserTag()
            {

                // https://api.fizzyo-ucl.co.uk/api/v1/users/:id

                string getTag = "https://api.fizzyo-ucl.co.uk/api/v1/users/" + PlayerPrefs.GetString("userId");

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));
                WWW sendGetTag = new WWW(getTag, null, headers);

                while (!sendGetTag.isDone) { }

                if (sendGetTag.error != null)
                {
                    PlayerPrefs.SetInt("tagLoaded", 0);
                    return;
                }

                PlayerPrefs.SetInt("tagLoaded", 1);

                UserTag allData = JsonUtility.FromJson<UserTag>(sendGetTag.text);

                if (Regex.IsMatch(allData.gamerTag, "^[A-Z]{3}$"))
                {
                    
                    PlayerPrefs.SetInt("tagDone", 1);
                    
                    return;
                } else
                {
                    PlayerPrefs.SetInt("tagDone", 0);
                    return;
                }


            }

            /// <summary>
            /// Loads in the top 20 highscores for the current game
            /// </summary>
            /// <returns>
            /// A JSON formatted string containing tag and score for the top 20 unlocked achievements
            /// </returns> 
            public static string GetHighscores()
            {

                if (PlayerPrefs.GetInt("online") == 0)
                {
                    return "Highscore Load Failed";
                }

                string getHighscores = "https://api.fizzyo-ucl.co.uk/api/v1/games/" + PlayerPrefs.GetString("gameId") + "/highscores";

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));
                WWW sendGetHighscores = new WWW(getHighscores, null, headers);

                while (!sendGetHighscores.isDone) { }

                if (sendGetHighscores.error != null)
                {
                    return "Highscore Load Failed";
                }

                return sendGetHighscores.text;
            }

            /// <summary>
            /// Sets up the player preferences to allow the user to play offline
            /// </summary>
            public static void PlayOffline()
            {
                ResetPlayerPrefs();
            }

            /// <summary>
            /// Resets all of the player preferences
            /// </summary>
            private static void ResetPlayerPrefs()
            {
                PlayerPrefs.SetInt("online", 0);
                PlayerPrefs.SetInt("calDone", 0);
                string path = Application.streamingAssetsPath + "/Achievements.json";
                string achJSONData = File.ReadAllText(path);
                PlayerPrefs.SetString("achievements", achJSONData);
                PlayerPrefs.SetString("achievementsToUpload", "");
                PlayerPrefs.SetString("achievementsToProgress", "");

                PlayerPrefs.DeleteKey("accessToken");
                PlayerPrefs.DeleteKey("tagDone");
                PlayerPrefs.DeleteKey("userTag");
                PlayerPrefs.DeleteKey("calPressure");
                PlayerPrefs.DeleteKey("calTime");
                PlayerPrefs.DeleteKey("userId");
                PlayerPrefs.DeleteKey("gameId");
                PlayerPrefs.DeleteKey("gameSecret");
                PlayerPrefs.DeleteKey("userLoaded");
                PlayerPrefs.DeleteKey("calLoaded");
                PlayerPrefs.DeleteKey("achLoaded");
                PlayerPrefs.DeleteKey("tagLoaded"); 
            }

        }

        public class Upload : MonoBehaviour
        {
            /// <summary>
            /// Uploads a player tag to the Fizzyo API
            /// </summary>
            /// <returns>
            /// String - "Tag Upload Complete" - If upload completes  
            /// String - "Tag Upload Failed" - If upload fails
            /// String - "Please Select A Different Tag" - If tag contains profanity
            /// </returns> 
            public static string UserTag(string tag)
            {

                if (PlayerPrefs.GetInt("online") == 0)
                {
                    return "Tag Upload Failed";
                }

                string[] tagFilter = { "ASS", "FUC", "FUK", "FUQ", "FUX", "FCK", "COC", "COK", "COQ", "KOX", "KOC", "KOK", "KOQ", "CAC", "CAK", "CAQ", "KAC", "KAK", "KAQ", "DIC", "DIK", "DIQ", "DIX", "DCK", "PNS", "PSY", "FAG", "FGT", "NGR", "NIG", "CNT", "KNT", "SHT", "DSH", "TWT", "BCH", "CUM", "CLT", "KUM", "KLT", "SUC", "SUK", "SUQ", "SCK", "LIC", "LIK", "LIQ", "LCK", "JIZ", "JZZ", "GAY", "GEY", "GEI", "GAI", "VAG", "VGN", "SJV", "FAP", "PRN", "LOL", "JEW", "JOO", "GVR", "PUS", "PIS", "PSS", "SNM", "TIT", "FKU", "FCU", "FQU", "HOR", "SLT", "JAP", "WOP", "KIK", "KYK", "KYC", "KYQ", "DYK", "DYQ", "DYC", "KKK", "JYZ", "PRK", "PRC", "PRQ", "MIC", "MIK", "MIQ", "MYC", "MYK", "MYQ", "GUC", "GUK", "GUQ", "GIZ", "GZZ", "SEX", "SXX", "SXI", "SXE", "SXY", "XXX", "WAC", "WAK", "WAQ", "WCK", "POT", "THC", "VAJ", "VJN", "NUT", "STD", "LSD", "POO", "AZN", "PCP", "DMN", "ORL", "ANL", "ANS", "MUF", "MFF", "PHK", "PHC", "PHQ", "XTC", "TOK", "TOC", "TOQ", "MLF", "RAC", "RAK", "RAQ", "RCK", "SAC", "SAK", "SAQ", "PMS", "NAD", "NDZ", "NDS", "WTF", "SOL", "SOB", "FOB", "SFU", "PEE", "DIE", "BUM", "BUT", "IRA" };

                if (tagFilter.Contains(tag) || !Regex.IsMatch(tag, "^[A-Z]{3}$"))
                {
                    return "Please Select A Different Tag";
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
                    return "Tag Upload Failed";
                }

                PlayerPrefs.SetInt("tagDone", 1);
                PlayerPrefs.SetString("userTag", tag);

                return "Tag Upload Complete";

            }

            /// <summary>
            /// Uploads a players calibration data and also sets the values in player prefs
            /// </summary>
            /// <returns>
            /// String - "Upload Complete" - If upload completes  
            /// String - "Upload Failed" - If upload fails
            /// </returns> 
            public static string Calibration(float pressure, float time)
            {

                PlayerPrefs.SetFloat("calPressure", pressure);
                PlayerPrefs.SetFloat("calTime", time);
                PlayerPrefs.SetInt("calDone", 1);

                if (PlayerPrefs.GetInt("online") == 0)
                {
                    return "Upload Failed";
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
                    return "Upload Failed";
                }

                return "Upload Complete";
            }

            /// <summary>
            /// Uploads a players Score
            /// </summary>
            /// <returns>
            /// String - "High Score Upload Complete" - If upload completes  
            /// String - "High Score Upload Failed" - If upload fails
            /// </returns>
            public static string Score(int score)
            {

                if (PlayerPrefs.GetInt("online") == 0)
                {
                    return "Score Upload Failed";
                }

                string uploadScore = "https://api.fizzyo-ucl.co.uk/api/v1/games/" + PlayerPrefs.GetString("gameId") + "/highscores";

                WWWForm form = new WWWForm();
                form.AddField("gameSecret", PlayerPrefs.GetString("gameSecret"));
                form.AddField("userId", PlayerPrefs.GetString("userId"));
                form.AddField("score", score);
                Dictionary<string, string> headers = form.headers;
                headers["Authorization"] = "Bearer " + PlayerPrefs.GetString("accessToken");

                byte[] rawData = form.data;

                WWW sendPostUnlock = new WWW(uploadScore, rawData, headers);

                while (!sendPostUnlock.isDone) { };

                if (sendPostUnlock.error != null)
                {
                    return "Score Upload Failed";
                }

                return "Score Upload Complete";
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
            /// <returns>
            /// String - "Session Upload Complete /nAchievement Upload Complete" - If session upload completes and achievement upload completes
            /// String - "Session Upload Complete /nAchievement Upload Failed" - If session upload completes and achievement upload fails
            /// String - "Session Upload Failed /nAchievement Upload Complete" - If session upload fails and achievement upload completes
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

                status += Achievements();

                return status;

            }

            /// <summary>
            /// Uploads a players achievements for a session
            /// </summary>
            /// <returns>
            /// String - "Achievement Upload Complete" - If upload completes  
            /// String - "Achievement Upload Failed" - If upload fails
            /// </returns>
            private static string Achievements()
            {

                string achievementsToUpload = PlayerPrefs.GetString("achievementsToUpload");

                if (achievementsToUpload != "")
                {

                    string[] achievementsToUploadArray = achievementsToUpload.Split(',');

                    for (int i = 0; i < achievementsToUploadArray.Length; i++)
                    {

                        if (achievementsToUploadArray[i] != "")
                        {

                            string postUnlock;

                            postUnlock = "https://api.fizzyo-ucl.co.uk/api/v1/game/" + PlayerPrefs.GetString("gameId") + "/achievements/" + achievementsToUploadArray[i] + "/unlock";

                            WWWForm form = new WWWForm();

                            form.AddField("gameSecret", PlayerPrefs.GetString("gameSecret"));
                            form.AddField("userId", PlayerPrefs.GetString("userId"));

                            Dictionary<string, string> headers = form.headers;
                            headers["Authorization"] = "Bearer " + PlayerPrefs.GetString("accessToken");

                            byte[] rawData = form.data;

                            WWW sendPostUnlock = new WWW(postUnlock, rawData, headers);

                            while (!sendPostUnlock.isDone) { }

                            if (sendPostUnlock.error != null)
                            {
                                return Environment.NewLine + "Achievement Upload Failed";
                            }

                        }

                    }

                }

                string achievementsToProgress = PlayerPrefs.GetString("achievementsToProgress");

                string[] achievementsToProgressArray = achievementsToProgress.Split(',');

                AllAchievementData allUserProgress = JsonUtility.FromJson<AllAchievementData>(PlayerPrefs.GetString(PlayerPrefs.GetString("userId") + "AchievementProgress"));
                AllAchievementData allAchievements = JsonUtility.FromJson<AllAchievementData>(PlayerPrefs.GetString("achievements"));

                // Add achievement progress to player preferences
                for (int i = 0; i < achievementsToProgressArray.Length; i++)
                {

                    if (achievementsToProgressArray[i] != "")
                    {

                        for (int j = 0; j < allUserProgress.achievements.Length; j++)
                        {

                            if (allUserProgress.achievements[j].id == achievementsToProgressArray[i])
                            {
                                for (int k = 0; k < allAchievements.achievements.Length; k++)
                                {

                                    if (allUserProgress.achievements[j].id == allAchievements.achievements[k].id)
                                    {
                                        allUserProgress.achievements[j].unlockProgress = allAchievements.achievements[k].unlockProgress;
                                        string newAllData = JsonUtility.ToJson(allUserProgress);
                                        PlayerPrefs.SetString(PlayerPrefs.GetString("userId") + "AchievementProgress", newAllData);
                                        break;
                                    }

                                }

                                break;
                            }

                        }

                    }

                }

                PlayerPrefs.SetString("achievementsToUpload", "");
                PlayerPrefs.SetString("achievementsToProgress", "");
                return Environment.NewLine + "Achievement Upload Complete";

            }



        }
        
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

        // Serializable which holds high score data
        [System.Serializable]
        public class AllHighscoreData
        {
            public HighscoreData[] highscores;

        }

        // Serializable which holds individual high score data
        [System.Serializable]
        public class HighscoreData
        {
            public string tag;
            public int score;
            public bool belongsToUser;
        }

        // Serializable which holds achievement data
        [System.Serializable]
        public class AllAchievementData
        {
            public AchievementData[] achievements;
            public AchievementData[] unlockedAchievements;
        }

        // Serializable that is used to pull and hold the data of each Achievement in the Achievements.json file
        [System.Serializable]
        public class AchievementData
        {
            public string category;
            public string id;
            public string title;
            public string description;
            public int points;
            public int unlock;
            public int unlockProgress;
            public int unlockRequirement;
            public string dependency;
            public string unlockedOn;
        }

        // Serializable which holds calibration data
        [System.Serializable]
        public class CalibrationData
        {
            public string calibratedOn;
            public float pressure;
            public int time;
        }

        // Serializable which holds user tag
        [System.Serializable]
        public class UserTag
        {
            public string gamerTag;
        }
    }

    /// <summary>
    /// An Instance of this class can be created to calibrate the game based on the users device input
    /// </summary>
    public class Calibration : MonoBehaviour
    {
        // Time that current breath has been held for
        private float breathLength;

        // How many calibration steps have been completed
        public int calibrationStep = 1;

        // How many calibration steps need to be completed
        public int requiredSteps = 3;

        // Status of calibration
        public string calibrationStatus;

        // A color reflecting of the status of the calibration
        public Color calibrationColor;

        // List that holds pressure readings from calibration
        private List<float> pressureReadings = new List<float>();

        // List that holds pressure readings from calibration
        private List<float> avgPressureReadings = new List<float>();

        // List that holds pressure readings from calibration
        private List<float> avgLengths = new List<float>();

        // Holds average pressure reading
        private float avgPressureReading;

        // Average time of each calibration step
        private float avgLength;

        // Breath has to be above this to register
        private float minPressureThreshold = 0.1f;

        // Pressure used for calibration from device
        public float pressure;

        // If true calibration script is running
        public bool calibrating = false;

        // If true calibration is finished
        public bool calibrationFinished = false;


        /// <summary>
        /// Used to get input from the device to get a pressure and time value that can be used in the breath framework, according to the breathing capacity of the user
        /// Pressure is a float value that determines how hard the user needs to blow into the device to constitute a good breath
        /// Time is an integer value that determines how long the user needs to blow into the device to constitute a good breath
        /// Calibration pressure and time are saved in the player preferences as "calPressure" and "calTime"
        /// </summary>
        public void Calibrate()
        {

            // Pressure comes from device
            pressure = FizzyoDevice.Instance().Pressure();

            // if incoming pressure is above threshold
            if (pressure > minPressureThreshold)
            {
                // Pressure readings are taken every update
                pressureReadings.Add(pressure);
                breathLength += Time.deltaTime;

                calibrationStatus = "Status: Calibraion Step " + calibrationStep.ToString() + " In Progress";
                calibrationColor = Color.yellow;
            }
            // If the pressure is not being maintained
            else
            {

                if (breathLength > 1.0)
                {

                    // Average and maximum values are taken from the calibration pressure readings
                    avgPressureReadings.Add(pressureReadings.Sum() / pressureReadings.Count);
                    avgLengths.Add(breathLength);

                    int step = calibrationStep - 1;

                    if (calibrationStep == requiredSteps)
                    {

                        avgPressureReading = avgPressureReadings.Sum() / avgPressureReadings.Count;
                        avgLength = avgLengths.Sum() / avgLengths.Count;

                        calibrationStatus = "Status: Uploading...";
                        calibrationColor = Color.green;

                        calibrationStatus = "Status: " + Data.Upload.Calibration(avgPressureReading, avgLength);
                        if (calibrationStatus == "Status: Upload Failed")
                        {
                            calibrationColor = Color.red;
                        } else
                        {
                            calibrationColor = Color.green;
                        }

                        calibrating = false;
                        calibrationFinished = true;

                    }
                    else
                    {

                        pressureReadings.Clear();
                        breathLength = 0;

                        calibrating = false;

                        calibrationStatus = "Status: Calibraion Step " + calibrationStep.ToString() + " Complete";
                        calibrationColor = Color.green;

                        calibrationStep += 1;

                    }

                }
                // If length of breath was less than 1
                else
                {

                    calibrating = false;

                    pressureReadings.Clear();
                    breathLength = 0;

                    // Reset countdown and text
                    calibrationStatus = "Status: Please Try Again";
                    calibrationColor = Color.red;
                }

            }

        }
    }

    public class Session : MonoBehaviour
    {
        // Various session parameters
        public int setCount;
        public int breathCount;
        public int goodBreathCount;
        public int badBreathCount;
        public int score;
        public int startTime;

        /// <summary>
        /// Constructor for a session
        /// </summary>
        /// <param name="setCount"> 
        /// Integer holding the amount of sets that are to be completed in this session
        /// </param>  
        /// <param name="breathCount"> 
        /// Integer holding the amount of breaths that are to be completed in each set
        /// </param>  
        public Session(int setCount, int breathCount)
        {

            this.setCount = setCount;
            this.breathCount = breathCount;

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            TimeSpan diff = DateTime.UtcNow - origin;
            this.startTime = (int)Math.Floor(diff.TotalSeconds);

        }

        /// <summary>
        /// Used to upload a session and achievements gained within this session
        /// </summary>
        /// <param name="goodBreathCount"> 
        /// Integer holding the amount of good breaths completed in this session
        /// </param>  
        /// <param name="badBreathCount"> 
        /// Integer holding the amount of bad breaths completed in this session
        /// </param>  
        /// /// <param name="score"> 
        /// Integer holding the score for this session
        /// </param>  
        public string SessionUpload(int goodBreathCount, int badBreathCount, int score)
        {

            return Data.Upload.Session(goodBreathCount, badBreathCount, score, startTime, setCount, breathCount);

        }
    }

    /// <summary>
    /// Provides data about the current breath to the receiver when the ExhalationComplete event fires
    /// </summary>
    public class ExhalationCompleteEventArgs : EventArgs
    {
        private float breathLength = 0;
        private int breathCount = 0;
        private float exhaledVolume = 0;
        private bool isBreathFull = false;
        private float breathPercentage = 0;
        private int breathQuality = 0;

        public ExhalationCompleteEventArgs(float breathLength, int breathCount, float exhaledVolume, bool isBreathFull, float breathPercentage, int breathQuality)
        {
            this.breathLength = breathLength;
            this.breathCount = breathCount;
            this.exhaledVolume = exhaledVolume;
            this.isBreathFull = isBreathFull;
            this.breathPercentage = breathPercentage;
            this.breathQuality = breathQuality;

        }

        /// The length of the exhaled breath in seconds
        public float Breathlength
        {
            get
            {
                return breathLength;
            }
        }

        /// The total number of exhaled breaths this session
        public int BreathCount
        {
            get
            {
                return breathCount;
            }
        }

        /// The total exhaled volume of this breath
        public float ExhaledVolume
        {
            get
            {
                return exhaledVolume;
            }
        }

        /// Returns true if the breath was 100% completed
        public bool IsBreathFull
        {
            get
            {
                return isBreathFull;
            }
        }

        /// Returns true if the breath was 100% completed
        public int BreathQuality
        {
            get
            {
                return breathQuality;
            }
        }

        /// Returns true if the breath was 100% completed
        public float BreathPercentage
        {
            get
            {
                return breathPercentage;
            }
        }
    }

    public delegate void ExhalationCompleteEventHandler(object sender, ExhalationCompleteEventArgs e);

    /// <summary>
    /// Breath Analyser class decouples the logic of recognizing breaths from a stream of pressure samples
    /// from acting on the recognition.  To use:
    /// 
    /// 1. Create an instance of BreathAnalyser: BreathAnalyser breathAnalyser = new BreathAnalyser()
    /// 2. Set the calibration properties: MaxPressure and MaxBreathLength
    /// 3. Register for the ExhalationCompleteEvent: breathAnalyser.ExhalationComplete += ExhalationCompleteHandler
    /// 4. Add pressure samples in the update loop: AddSample(Time.DeltaTime, pressure)
    /// 5. The event will fire at the end of an exhaled breath and provide information for:
    /// 
    ///    a) BreathLength
    ///    b) BreathCount
    ///    c) ExhaledVolume
    ///    d) IsBreathFull
    /// 
    /// 6. You can interrogate the breath analyser at any time to determine:
    /// 
    ///    a) BreathLength
    ///    b) BreathCount
    ///    c) ExhaledVolume
    ///    d) IsExhaling
    ///    e) MaxPressure
    ///    f) MaxBreathLength
    /// 
    /// The algorithm for determining whether a breath is fully completed is encapsulated in the method IsBreathFull()
    /// and currently returns true if the average breath pressure and breath length is within 80% of the max.
    /// </summary>
    public class BreathRecogniser
    {
        private float breathLength = 0;
        private int breathCount = 0;
        private float exhaledVolume = 0;
        private bool isExhaling = false;
        private float maxPressure = 0;
        private float maxBreathLength = 0;
        private const float kTollerance = 0.80f;
        private float minBreathThreshold = .05f;
        private float breathPercentage = 0;

        public event ExhalationCompleteEventHandler ExhalationComplete;


        public BreathRecogniser()
        {

            maxPressure = PlayerPrefs.GetFloat("calPressure");
            maxBreathLength = PlayerPrefs.GetFloat("calTime");

        }

        /// The length of the current exhaled breath in seconds
        public float Breathlength
        {
            get
            {
                return this.breathLength;
            }
        }

        /// The total number of exhaled breaths this session
        public int BreathCount
        {
            get
            {
                return this.breathCount;
            }
        }

        /// The total exhaled volume for this breath
        public float ExhaledVolume
        {
            get
            {
                return this.exhaledVolume;
            }
        }

        /// True if the user is exhaling
        public bool IsExhaling
        {
            get
            {
                return this.isExhaling;
            }
        }

        /// The maximum pressure recorded during calibration
        public float MaxPressure
        {
            get
            {
                return this.maxPressure;
            }
            set
            {
                this.maxPressure = value;
            }
        }

        /// The maximum breath length recorded during calibration
        public float MaxBreathLength
        {
            get
            {
                return this.maxBreathLength;
            }
            set
            {
                this.maxBreathLength = value;
            }
        }

        /// True if the user is exhaling
        public float BreathPercentage
        {
            get
            {
                return this.breathPercentage;
            }
        }

        /// Adds a sample to the BreathAnalyser
        public void AddSample(float dt, float value)
        {
           
            if (this.isExhaling && value < this.minBreathThreshold)
            {
                // Notify the delegate that the exhaled breath is complete
                bool isBreathFull = this.IsBreathFull(this.breathLength, this.maxBreathLength, this.exhaledVolume, this.maxPressure);
                int breathQuality = GetBreathQuality(this.breathPercentage);
                ExhalationCompleteEventArgs eventArgs = new ExhalationCompleteEventArgs(
                    this.breathLength,
                    this.breathCount,
                    this.exhaledVolume,
                    isBreathFull,
                    this.breathPercentage,
                    breathQuality);
                this.OnExhalationComplete(this, eventArgs);

                // Reset the state
                this.breathLength = 0;
                this.exhaledVolume = 0;
                this.isExhaling = false;
                this.breathCount++;
                this.breathPercentage = 0;
            }
            else if (value >= this.minBreathThreshold)
            {
                this.isExhaling = true;
                this.exhaledVolume += dt * value;
                this.breathLength += dt;
                this.breathPercentage = this.breathLength / (BreathRecogniser.kTollerance * this.maxBreathLength);
            }
        }

        /// Returns true if the breath was within the toterance of a 'good breath'
        public bool IsBreathFull(float breathLength, float maxBreathLength, float exhaledVolume, float maxPressure)
        {
            bool isBreathFull = false;

            // Is the breath the right within 80% of the correct length
            isBreathFull = breathLength > BreathRecogniser.kTollerance * maxBreathLength;

            // Is the average pressure within 80% of the max pressure
            if (this.breathLength > 0)
            {
                isBreathFull = isBreathFull && ((exhaledVolume / breathLength) > BreathRecogniser.kTollerance * maxPressure);
            }

            return isBreathFull;
        }

        /// Returns an integer that corresponds to the following:
        /// 0 - Breath was 0 - 25% of the calibrated breath length
        /// 1 - Breath was 25% - 50% of the calibrated breath length
        /// 2 - Breath was 50 - 75% of the calibrated breath length
        /// 3 - Breath was 75% - 100% of the calibrated breath length
        /// 4 - Breath was 100% of the calibrated breath length
        public int GetBreathQuality(float breathPercentage)
        {
            int quality = 0;

            if (breathPercentage < 0.5f && breathPercentage > 0.25f)
                quality = 1;
            else if (breathPercentage < 0.75f && breathPercentage > 0.5f)
                quality = 2;
            else if (breathPercentage < 1.0f && breathPercentage > 0.75f)
                quality = 3;
            else if (breathPercentage >= 1.0f)
                quality = 4;

            return quality;
        }

        /// Resest the BreathAnalyser
        public void ResetSession()
        {
            this.breathLength = 0;
            this.breathCount = 0;
            this.exhaledVolume = 0;
            this.isExhaling = false;
            this.breathPercentage = 0;
        }

        /// Invoke the event - called whenever exhalation finishes
        protected virtual void OnExhalationComplete(object sender, ExhalationCompleteEventArgs e)
        {
            if (ExhalationComplete != null)
            {
                ExhalationComplete(this, e);
            }
        }
    }
}
