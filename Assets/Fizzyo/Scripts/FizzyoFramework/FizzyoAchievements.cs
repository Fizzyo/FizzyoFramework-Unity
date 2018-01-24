using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Loads all game achievements and the users unlocked achievements and achievement progres.
        /// </summary>
		/// <returns>
        /// A JSON formatted string containing the list of achievements
        /// </returns> 
        public FizzyoRequestReturnType LoadAchievements()
        {
            //Get all achievements from server
            string getAchievements = "https://api.fizzyo-ucl.co.uk/api/v1/games/" + FizzyoFramework.Instance.gameID + "/achievements"; 

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Bearer " + FizzyoFramework.Instance.User.AccessToken);
            WWW sendGetAchievements = new WWW(getAchievements, null, headers);

            while (!sendGetAchievements.isDone) { }

            string achievementsJSONData = sendGetAchievements.text;
            allAchievements = JsonUtility.FromJson<AllAchievementData>(achievementsJSONData).achievements;
             
            //get unlocked achievements
            string getUnlock = "https://api.fizzyo-ucl.co.uk/api/v1/users/" + FizzyoFramework.Instance.User.UserID + "/unlocked-achievements/" + FizzyoFramework.Instance.gameID;

            headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Bearer " + FizzyoFramework.Instance.User.AccessToken);
            WWW sendGetUnlock = new WWW(getUnlock, null, headers);

            while (!sendGetUnlock.isDone) { }

            if(sendGetUnlock.error != null)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            string unlockedJSONData = sendGetUnlock.text;
            unlockedAchievements = JsonUtility.FromJson<AllAchievementData>(unlockedJSONData).achievements;


            return FizzyoRequestReturnType.SUCCESS;

        }

        internal void Load()
        {
            LoadAchievements();
        }


        /// <summary>
        /// Loads in the top 20 highscores for the current game
        /// </summary>
        /// <returns>
        /// A JSON formatted string containing tag and score for the top 20 scores of the game
        /// </returns> 
        public FizzyoRequestReturnType GetHighscores()
        {
            string getHighscores = "https://api.fizzyo-ucl.co.uk/api/v1/games/" + PlayerPrefs.GetString("gameId") + "/highscores";

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));
            WWW sendGetHighscores = new WWW(getHighscores, null, headers);

            while (!sendGetHighscores.isDone) { }

            if (sendGetHighscores.error != null)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

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
               return FizzyoRequestReturnType.FAILED_TO_CONNECT;
            }

            return FizzyoRequestReturnType.SUCCESS;
        }


        /// <summary>
        /// Unlocks the achievement specified in the parameter, its achievementID. 
        /// </summary>
        /// <returns>
        /// String - "Achievement Upload Complete" - If upload completes  
        /// String - "Achievement Upload Failed" - If upload fails
        /// </returns>
        /// 
        public FizzyoRequestReturnType UnlockAchievement(string achievementId)
        {
            string unlockAchievement = "https://api.fizzyo-ucl.co.uk/api/v1/games/" + PlayerPrefs.GetString("gameId") + "/achievements/" + achievementId + "/unlock" ;

            WWWForm form = new WWWForm();
            form.AddField("gameSecret", PlayerPrefs.GetString("gameSecret"));
            form.AddField("userId", PlayerPrefs.GetString("userId"));
            form.AddField("achievementId", achievementId);
            Dictionary<string, string> headers = form.headers;
            headers["Authorization"] = "Bearer " + PlayerPrefs.GetString("accessToken");

            byte[] rawData = form.data;

            WWW sendPostUnlock = new WWW(unlockAchievement, rawData, headers);

            while (!sendPostUnlock.isDone) { };

            if (sendPostUnlock.error != null)
            {
                return FizzyoRequestReturnType.FAILED_TO_CONNECT;
                //TODO add upload que here
            }
            return FizzyoRequestReturnType.SUCCESS;
        }

        /// <summary>
        /// Uploads a players achievements for a session
        /// </summary>
        /// <returns>
        /// String - "Achievement Upload Complete" - If upload completes  
        /// String - "Achievement Upload Failed" - If upload fails
        /// </returns>
        private FizzyoRequestReturnType PostAchievements()
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
                            return FizzyoRequestReturnType.FAILED_TO_CONNECT;
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
               return FizzyoRequestReturnType.SUCCESS;
        }

    }

}

