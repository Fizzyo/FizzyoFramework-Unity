// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace Fizzyo
{

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


    /// <summary>
    /// Used to unlock Fizzyo achievements and post high scores in the Fizzyo rest API 
    /// </summary>
    public class FizzyoAchievements 
    {
        /// <summary>
        /// Array of type AchievementData which holds all the achievements that the game has to offer. 
        /// </summary>
        public AchievementData[] allAchievements;
		/// <summary>
		/// Array of type AchievementData which holds the achievements the user has unlocked. 
		/// </summary>
        public AchievementData[] unlockedAchievements;

        /// <summary>
        /// Array of type HighscoreData which holds the top 20 highest scores for the game.
        /// </summary>
        public HighscoreData[] highscores;

        /// <summary>
        /// Loads all game achievements and the users unlocked achievements and achievement progress.
        /// </summary>
		/// <returns>
        /// A JSON formatted string containing the list of achievements
        /// </returns> 
        public FizzyoRequestReturnType LoadAchievements()
        {
            if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            //Get all achievements from server
            var webRequest = FizzyoNetworking.GetWebRequest(FizzyoNetworking.ApiEndpoint + "games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/achievements");
            webRequest.SendWebRequest();

            //string getAchievements = FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + "/api/v1/games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/achievements"; 

            //Dictionary<string, string> headers = new Dictionary<string, string>();
            //headers.Add("Authorization", "Bearer " + FizzyoFramework.Instance.User.AccessToken);
            //headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");

            //WWW sendGetAchievements = new WWW(getAchievements, null, headers);

            while (!webRequest.isDone) { }

            string achievementsJSONData = webRequest.downloadHandler.text;
            allAchievements = JsonUtility.FromJson<AllAchievementData>(achievementsJSONData).achievements;

            //get unlocked achievements
            webRequest = FizzyoNetworking.GetWebRequest(FizzyoNetworking.ApiEndpoint + "users/" + FizzyoFramework.Instance.User.UserID + "/unlocked-achievements/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID);
            webRequest.SendWebRequest();

            //string getUnlock = FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + "/api/v1/users/" + FizzyoFramework.Instance.User.UserID + "/unlocked-achievements/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID;

            //headers = new Dictionary<string, string>();
            //headers.Add("Authorization", "Bearer " + FizzyoFramework.Instance.User.AccessToken);
            //headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
            //WWW sendGetUnlock = new WWW(getUnlock, null, headers);

            while (!webRequest.isDone) { }

            if(webRequest.error != null)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            string unlockedJSONData = webRequest.downloadHandler.text;
            unlockedAchievements = JsonUtility.FromJson<AllAchievementData>(unlockedJSONData).unlockedAchievements;


            return FizzyoRequestReturnType.SUCCESS;
        }

        internal void Load()
        {
            LoadAchievements();
        }

        /// <summary>
        /// Loads in the top 20 high-scores for the current game.
        /// </summary>
        /// <returns>
        /// A JSON formatted string containing tag and score for the top 20 scores of the game
        /// </returns> 
        public FizzyoRequestReturnType GetHighscores()
        {
            if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }
                var webRequest = FizzyoNetworking.GetWebRequest(FizzyoNetworking.ApiEndpoint + "games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/highscores");
            webRequest.SendWebRequest();

            //string getHighscores = FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + "/api/v1/games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/highscores";

            //Dictionary<string, string> headers = new Dictionary<string, string>();
            //headers.Add("Authorization", "Bearer " + FizzyoFramework.Instance.User.AccessToken);
            //headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
            //WWW sendGetHighscores = new WWW(getHighscores, null, headers);

            while (!webRequest.isDone) { }

            if (webRequest.error != null)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            string topScoresJSONData = webRequest.downloadHandler.text;
            highscores = JsonUtility.FromJson<AllHighscoreData>(topScoresJSONData).highscores;

            return FizzyoRequestReturnType.SUCCESS;
        }

        /// <summary>
        /// Uploads a players Score
        /// </summary>
        /// <returns>
        /// String - "High Score Upload Complete" - If upload completes  
        /// String - "High Score Upload Failed" - If upload fails
        /// </returns>
        public FizzyoRequestReturnType PostScore(int score)
        {
            if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("gameSecret", FizzyoFramework.Instance.FizzyoConfigurationProfile.GameSecret);
            formData.Add("userId", FizzyoFramework.Instance.User.UserID);
            formData.Add("score", score.ToString());

            var webRequest = FizzyoNetworking.PostWebRequest(FizzyoNetworking.ApiEndpoint + "games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/highscores", formData);
            webRequest.SendWebRequest();

            //string uploadScore = FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + "/api/v1/games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/highscores";

            //WWWForm form = new WWWForm();
            //form.AddField("gameSecret", FizzyoFramework.Instance.FizzyoConfigurationProfile.GameSecret);
            //form.AddField("userId", FizzyoFramework.Instance.User.UserID);
            //form.AddField("score", score);
            //Dictionary<string, string> headers = form.headers;
            //headers["Authorization"] = "Bearer " + FizzyoFramework.Instance.User.AccessToken;
            //headers["User-Agent"] =  "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";


            //byte[] rawData = form.data;

            //WWW sendPostUnlock = new WWW(uploadScore, rawData, headers);

            while (!webRequest.isDone) { };

            if (webRequest.error != null)
            {
               return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            return FizzyoRequestReturnType.SUCCESS;
        }


        /// <summary>
        /// Unlocks the achievement specified in the parameter, its achievementID. 
        /// </summary>
        /// <returns>
        /// FizzyoRequestReturnType.SUCCESS is upload is successful.  
        /// FizzyoRequestReturnType.FAILED_TO_CONNECT if connection failed.  
        /// </returns>
        public FizzyoRequestReturnType UnlockAchievement(string achievementId)
        {
            if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("gameSecret", FizzyoFramework.Instance.FizzyoConfigurationProfile.GameSecret);
            formData.Add("userId", FizzyoFramework.Instance.User.UserID);
            formData.Add("achievementId", achievementId);

            var webRequest = FizzyoNetworking.PostWebRequest(FizzyoNetworking.ApiEndpoint + "games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/achievements/" + achievementId + "/unlock", formData);
            webRequest.SendWebRequest();

            //string unlockAchievement = FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + "/api/v1/games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/achievements/" + achievementId + "/unlock" ;

            //WWWForm form = new WWWForm();
            //form.AddField("gameSecret", FizzyoFramework.Instance.FizzyoConfigurationProfile.GameSecret);
            //form.AddField("userId", FizzyoFramework.Instance.User.UserID);
            //form.AddField("achievementId", achievementId);
            //Dictionary<string, string> headers = form.headers;
            //headers["Authorization"] = "Bearer " + FizzyoFramework.Instance.User.AccessToken;
            //headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

            //byte[] rawData = form.data;

            //WWW sendPostUnlock = new WWW(unlockAchievement, rawData, headers);

            while (!webRequest.isDone) { };

            if (webRequest.error != null)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            return FizzyoRequestReturnType.SUCCESS;
        }

        /// <summary>
        /// Uploads a players achievements for a session
        /// </summary>
        /// <returns>
        /// FizzyoRequestReturnType.SUCCESS is upload is successful.  
        /// FizzyoRequestReturnType.FAILED_TO_CONNECT if connection failed.  
        /// </returns>
        private FizzyoRequestReturnType PostAchievements()
        {
            if (FizzyoNetworking.loginResult != LoginReturnType.SUCCESS)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            string achievementsToUpload = PlayerPrefs.GetString("achievementsToUpload");

            if (achievementsToUpload != "")
            {
                string[] achievementsToUploadArray = achievementsToUpload.Split(',');

                for (int i = 0; i < achievementsToUploadArray.Length; i++)
                {
                    if (achievementsToUploadArray[i] != "")
                    {
                        Dictionary<string, string> formData = new Dictionary<string, string>();
                        formData.Add("gameSecret", FizzyoFramework.Instance.FizzyoConfigurationProfile.GameSecret);
                        formData.Add("userId", FizzyoFramework.Instance.User.UserID);

                        var webRequest = FizzyoNetworking.PostWebRequest(FizzyoNetworking.ApiEndpoint + "games/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/achievements/" + achievementsToUploadArray[i] + "/unlock", formData);
                        webRequest.SendWebRequest();

                        //string postUnlock;

                        //postUnlock = FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + "/api/v1/game/" + FizzyoFramework.Instance.FizzyoConfigurationProfile.GameID + "/achievements/" + achievementsToUploadArray[i] + "/unlock";

                        //WWWForm form = new WWWForm();

                        //form.AddField("gameSecret", FizzyoFramework.Instance.FizzyoConfigurationProfile.GameSecret);
                        //form.AddField("userId", FizzyoFramework.Instance.User.UserID);

                        //Dictionary<string, string> headers = form.headers;
                        //headers["Authorization"] = "Bearer " + FizzyoFramework.Instance.User.AccessToken;
                        //headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

                        //byte[] rawData = form.data;

                        //WWW sendPostUnlock = new WWW(postUnlock, rawData, headers);

                        while (!webRequest.isDone) { }

                        if (webRequest.error != null)
                        {
                            return FizzyoRequestReturnType.FAILED_TO_CONNECT;
                        }
                    }
                }
            }

            string achievementsToProgress = PlayerPrefs.GetString("achievementsToProgress");

            string[] achievementsToProgressArray = achievementsToProgress.Split(',');

            AllAchievementData allUserProgress = JsonUtility.FromJson<AllAchievementData>(PlayerPrefs.GetString(FizzyoFramework.Instance.User.UserID + "AchievementProgress"));
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
                                    PlayerPrefs.SetString(FizzyoFramework.Instance.User.UserID + "AchievementProgress", newAllData);
                                    break;
                                }
                            }

                            break;
                        }
                    }
                }
            }
               return FizzyoRequestReturnType.SUCCESS;
        }
    }
}